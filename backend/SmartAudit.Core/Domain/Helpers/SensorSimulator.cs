using SmartAudit.Core.Domain.Enums;

namespace SmartAudit.Core.Domain.Helpers
{
    // Deterministická simulace telemetrie. Stejný vstup = stejný výstup, takže historie
    // dopočítaná při startu a živě přidávané hodnoty na sebe přesně navazují.
    //
    // Model je záměrně jednoduchý a vysvětlitelný:
    //  - teplota: zóna se blíží cílové teplotě s časovou konstantou budovy,
    //    cíl = žádaná teplota podle rozvrhu vytápění, nebo volně plovoucí teplota (venku + zisky), je-li vyšší
    //  - CO₂: roste k ustálené hodnotě podle obsazenosti, o přestávkách a v noci klesá
    //  - příkon: tepelná ztráta UA·ΔT + ohřev hmoty budovy + elektřina podle obsazenosti
    public static class SensorSimulator
    {
        private const int InnerStepMinutes = 5;
        private const double OutdoorCo2 = 420;

        public static IEnumerable<(DateTimeOffset Timestamp, double Value)> Generate(SensorSimulationSpec spec, DateTimeOffset from, DateTimeOffset to, int stepMinutes, Func<DateTimeOffset, double> outdoorTemperature)
        {
            if (stepMinutes < InnerStepMinutes || stepMinutes % InnerStepMinutes != 0)
                throw new ArgumentException("Krok simulace musí být násobkem 5 minut.", nameof(stepMinutes));

            var alignedFrom = AlignUp(from, stepMinutes);
            var state = new SimulationState();

            // Den náběhu, aby stav (teplota hmoty, CO₂) v okamžiku from odpovídal historii
            for (var t = alignedFrom.AddHours(-24); t <= to; t = t.AddMinutes(InnerStepMinutes))
            {
                var outdoor = outdoorTemperature(t);

                Step(spec, state, t, outdoor);

                if (t < alignedFrom || !IsAligned(t, stepMinutes))
                    continue;

                yield return (t, Output(spec, state, t));
            }
        }

        // Energie na vytápění a provoz v kWh za období — pro výpočet ztráty proti správnému nastavení
        public static double TotalEnergy(SensorSimulationSpec powerSpec, DateTimeOffset from, DateTimeOffset to, Func<DateTimeOffset, double> outdoorTemperature)
        {
            if (powerSpec.Metric != SensorMetric.Power)
                throw new ArgumentException("Energii lze spočítat jen pro senzor příkonu.", nameof(powerSpec));

            return Generate(powerSpec, from, to, 60, outdoorTemperature).Sum(r => r.Value);
        }

        // Náhradní venkovní teplota, když není k dispozici Open-Meteo (typické září v KHK)
        public static double FallbackOutdoorTemperature(DateTimeOffset time)
        {
            var local = SchoolCalendar.ToLocal(time);
            var hour = local.Hour + local.Minute / 60.0;
            var dayWave = Noise(7, local.DayOfYear) * 3;

            return 12 + dayWave + 5.5 * Math.Sin((hour - 9) / 24 * 2 * Math.PI);
        }

        #region Model

        private sealed class SimulationState
        {
            public double? ZoneTemperature;
            public double? BuildingTemperature;
            public double Co2 = OutdoorCo2 + 30;
            public double HeatingPower;
            public double ElectricPower;
        }

        private static void Step(SensorSimulationSpec spec, SimulationState state, DateTimeOffset time, double outdoor)
        {
            var local = SchoolCalendar.ToLocal(time);
            var day = DateOnly.FromDateTime(local);
            var minute = local.Hour * 60 + local.Minute;
            var dtHours = InnerStepMinutes / 60.0;

            var zoneType = spec.ZoneType ?? DefaultZone(spec.Category);
            var occupancy = Occupancy(spec.Category, zoneType, day, minute) * spec.OccupancyFactor;
            var setpoint = Setpoint(spec.Category, spec.Profile, zoneType, day, minute);

            // Teplota zóny
            var gains = 2.5 + 2.0 * Math.Min(occupancy, 1.3) + Solar(spec.Seed, minute);
            var freeRunning = outdoor + gains;
            var zoneTarget = Math.Min(27.5, Math.Max(setpoint, freeRunning));
            var zoneTemperature = state.ZoneTemperature ?? zoneTarget;
            var tauHours = zoneTarget > zoneTemperature ? (setpoint >= zoneTarget ? 1.5 : 5) : 10;

            state.ZoneTemperature = zoneTemperature + (zoneTarget - zoneTemperature) * Math.Min(1, dtHours / tauHours);

            // CO₂ zóny
            var rise = spec.IsProblemZone ? 1550 : spec.Profile == SimulationProfile.PoorVentilation ? 950 : 700;
            var steadyState = OutdoorCo2 + occupancy * rise;
            var co2Tau = steadyState > state.Co2 ? 0.6 : CO2DecayHours(spec, zoneType, day, minute);

            state.Co2 += (steadyState - state.Co2) * Math.Min(1, dtHours / co2Tau);

            // Celá budova — průměrný rozvrh (učebny / pokoje / expozice)
            var buildingZone = DefaultZone(spec.Category);
            var buildingSetpoint = Setpoint(spec.Category, spec.Profile, buildingZone, day, minute);
            var buildingOccupancy = Occupancy(spec.Category, buildingZone, day, minute);
            var buildingFreeRunning = outdoor + 3.5 + 2 * buildingOccupancy;
            var buildingTarget = Math.Max(buildingSetpoint, buildingFreeRunning);
            var buildingTemperature = state.BuildingTemperature ?? buildingTarget;
            var buildingTau = buildingTarget > buildingTemperature ? 2.0 : 10;
            var nextBuildingTemperature = buildingTemperature + (buildingTarget - buildingTemperature) * Math.Min(1, dtHours / buildingTau);

            // Topí se jen tehdy, když by bez topení budova nedosáhla žádané teploty
            var heatingActive = buildingSetpoint > buildingFreeRunning && buildingSetpoint > buildingTemperature - 0.3;
            var ua = spec.FloorArea * spec.SpecificHeatLoss / 1000;
            var capacity = spec.FloorArea * 0.04;
            var internalGains = spec.FloorArea * (1.5 + 5 * buildingOccupancy) / 1000;
            var loss = ua * Math.Max(0, nextBuildingTemperature - outdoor);
            var warmUp = capacity * Math.Max(0, nextBuildingTemperature - buildingTemperature) / dtHours;

            state.BuildingTemperature = nextBuildingTemperature;
            state.HeatingPower = heatingActive ? Math.Max(0, loss + warmUp - internalGains) : 0;
            state.ElectricPower = spec.FloorArea * (3 + 7 * buildingOccupancy) / 1000;
        }

        private static double Output(SensorSimulationSpec spec, SimulationState state, DateTimeOffset time)
        {
            var tick = time.ToUnixTimeSeconds() / 60;

            return spec.Metric switch
            {
                SensorMetric.Temperature => Math.Round((state.ZoneTemperature ?? 20) + ZoneOffset(spec.Seed) + Noise(spec.Seed, tick) * 0.12, 1),
                SensorMetric.Co2 => Math.Round(Math.Max(400, state.Co2 + Noise(spec.Seed, tick) * 22)),
                _ => Math.Round((state.HeatingPower + state.ElectricPower) * (1 + Noise(spec.Seed, tick) * 0.03), 1),
            };
        }

        #endregion

        #region Provozní rozvrhy

        private static readonly (int Start, int End)[] Lessons =
        [
            (8 * 60, 8 * 60 + 45), (8 * 60 + 55, 9 * 60 + 40), (10 * 60, 10 * 60 + 45),
            (10 * 60 + 55, 11 * 60 + 40), (11 * 60 + 50, 12 * 60 + 35), (12 * 60 + 45, 13 * 60 + 30),
            (13 * 60 + 40, 14 * 60 + 25),
        ];

        private static ZoneType DefaultZone(FacilityCategory category)
        {
            return category switch
            {
                FacilityCategory.School => ZoneType.Classroom,
                FacilityCategory.Social => ZoneType.ResidentRoom,
                FacilityCategory.Health => ZoneType.Ward,
                _ => ZoneType.Exhibition,
            };
        }

        private static double Occupancy(FacilityCategory category, ZoneType zone, DateOnly day, int minute)
        {
            var schoolDay = SchoolCalendar.IsSchoolDay(day);
            var workday = SchoolCalendar.IsWorkday(day);
            var inLesson = Lessons.Any(l => minute >= l.Start && minute < l.End);
            var schoolHours = minute >= 7 * 60 + 45 && minute < 14 * 60 + 30;

            return zone switch
            {
                ZoneType.Classroom or ZoneType.Workshop when schoolDay && inLesson => minute >= 13 * 60 + 40 ? 0.5 : 1.0,
                ZoneType.Classroom or ZoneType.Workshop when schoolDay && schoolHours => 0.3,
                ZoneType.Canteen when schoolDay && minute >= 11 * 60 + 30 && minute < 13 * 60 + 45 => 0.8,
                ZoneType.Gym when schoolDay && inLesson => 0.6,
                ZoneType.Gym when workday && minute >= 16 * 60 && minute < 20 * 60 => 0.35,
                ZoneType.Dormitory => DormitoryOccupancy(day, minute),
                ZoneType.Office when workday && minute >= 7 * 60 && minute < 15 * 60 + 30 => 1.0,
                ZoneType.ResidentRoom => minute >= 22 * 60 || minute < 6 * 60 ? 1.0 : 0.45,
                ZoneType.CommonRoom => minute >= 9 * 60 && minute < 19 * 60 + 30 ? 0.75 : 0,
                ZoneType.Ward => minute >= 21 * 60 || minute < 6 * 60 ? 0.9 : 0.7,
                ZoneType.Exhibition when day.DayOfWeek != DayOfWeek.Monday && minute >= 9 * 60 && minute < 17 * 60 => 0.3,
                ZoneType.Hall when day.DayOfWeek is DayOfWeek.Friday or DayOfWeek.Saturday && minute >= 19 * 60 && minute < 22 * 60 => 0.9,
                _ => 0,
            };
        }

        // Domov mládeže: žáci jsou tam přes noc během školního týdne (neděle večer – pátek ráno)
        private static double DormitoryOccupancy(DateOnly day, int minute)
        {
            var eveningBeforeSchool = SchoolCalendar.IsSchoolDay(day.AddDays(1)) && minute >= 17 * 60;
            var morningOfSchool = SchoolCalendar.IsSchoolDay(day) && minute < 7 * 60;
            var afternoonAfterSchool = SchoolCalendar.IsSchoolDay(day) && SchoolCalendar.IsSchoolDay(day.AddDays(1)) && minute >= 15 * 60;

            if (eveningBeforeSchool || morningOfSchool)
                return 1.0;

            return afternoonAfterSchool ? 0.6 : 0;
        }

        private static double Setpoint(FacilityCategory category, SimulationProfile profile, ZoneType zone, DateOnly day, int minute)
        {
            var schoolDay = SchoolCalendar.IsSchoolDay(day);

            if (profile == SimulationProfile.WeekendOverheating && SchoolCalendar.IsFreeDay(category, day))
                return 24.5;

            var night = minute < 5 * 60 || minute >= 21 * 60;

            switch (category)
            {
                case FacilityCategory.Social:
                case FacilityCategory.Health:
                    // Běžný noční útlum o 2 °C; nefunkční útlum drží 22 °C i v noci
                    return night && profile != SimulationProfile.NoNightSetback ? 20 : 22;

                case FacilityCategory.Culture:
                    var open = day.DayOfWeek != DayOfWeek.Monday && minute >= 7 * 60 + 30 && minute < 17 * 60;
                    return open || zone == ZoneType.Hall && minute >= 17 * 60 && minute < 22 * 60 ? 20 : 17;
            }

            if (zone == ZoneType.Dormitory)
                return DormitoryOccupancy(day, minute) > 0 || DormitoryOccupancy(day, minute + 60) > 0 ? 21 : 18;

            var heatingStart = profile == SimulationProfile.EarlyHeatingStart ? 3 * 60 + 45 : 5 * 60;
            var comfort = schoolDay && minute >= heatingStart && minute < 16 * 60;

            if (comfort)
                return zone == ZoneType.Gym ? 18.5 : 21;

            // Nefunkční noční útlum — v pracovní dny drží komfortní teplotu i v noci
            if (profile == SimulationProfile.NoNightSetback && schoolDay)
                return 21;

            return 18;
        }

        private static double CO2DecayHours(SensorSimulationSpec spec, ZoneType zone, DateOnly day, int minute)
        {
            var poorlyVentilated = spec.IsProblemZone || spec.Profile == SimulationProfile.PoorVentilation;

            // O přestávce se větrá okny
            if (spec.Category == FacilityCategory.School && zone == ZoneType.Classroom && SchoolCalendar.IsSchoolDay(day) && minute < 15 * 60)
                return poorlyVentilated ? 0.5 : 0.18;

            return poorlyVentilated ? 2.5 : 1.2;
        }

        // Sluneční zisk jižních místností kolem poledne (0–2,5 °C)
        private static double Solar(int seed, int minute)
        {
            var southFacing = Math.Abs(seed) % 3 == 0;
            if (!southFacing || minute < 9 * 60 || minute > 17 * 60)
                return 0;

            return 2.5 * Math.Sin((minute - 9 * 60) / (8.0 * 60) * Math.PI);
        }

        #endregion

        #region Pomocné

        private static double ZoneOffset(int seed) => Noise(seed, 1) * 0.6;

        // SplitMix64 → deterministický šum v intervalu <-1, 1>
        private static double Noise(long seed, long tick)
        {
            var z = unchecked((ulong)seed * 0x9E3779B97F4A7C15UL + (ulong)tick * 0xBF58476D1CE4E5B9UL);
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            z ^= z >> 31;

            return (z >> 11) * (1.0 / (1UL << 53)) * 2 - 1;
        }

        private static DateTimeOffset AlignUp(DateTimeOffset time, int stepMinutes)
        {
            var utc = time.ToUniversalTime();
            var stepTicks = TimeSpan.FromMinutes(stepMinutes).Ticks;
            var remainder = utc.Ticks % stepTicks;

            return remainder == 0 ? utc : new DateTimeOffset(utc.Ticks - remainder + stepTicks, TimeSpan.Zero);
        }

        private static bool IsAligned(DateTimeOffset time, int stepMinutes)
        {
            return time.UtcTicks % TimeSpan.FromMinutes(stepMinutes).Ticks == 0;
        }

        #endregion
    }
}
