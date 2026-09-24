using System.Globalization;
using SmartAudit.Core.Domain.Enums;

namespace SmartAudit.Core.Domain.Helpers
{
    public record HourlyValue(Guid? ZoneId, SensorMetric Metric, DateTimeOffset Hour, double Value);

    public record AuditFinding(string Title, string Detail, string Tag, AuditStatus Severity);

    public record AuditEvaluation(
        AuditStatus Status,
        string Headline,
        string Summary,
        IReadOnlyList<AuditFinding> Findings,
        double? LatestTemperature,
        double? LatestCo2,
        double? LatestPower,
        double Co2HoursOverLimit,
        double? FreeDayMeanTemperature,
        double? NightMeanTemperature,
        double WeeklyEnergyKwh,
        bool HeatingAnomaly,
        // Která zjištění vytápění audit našel — podle nich se z naměřených hodin počítá ztráta (HeatingLossEstimator)
        bool FreeDayOverheating,
        bool NightSetbackMissing);

    // Vysvětlitelná pravidla nad hodinovými průměry posledních 7 dní — žádné ML (F39, F24)
    public static class FacilityStatusEvaluator
    {
        private static readonly CultureInfo Czech = CultureInfo.GetCultureInfo("cs-CZ");

        public static AuditEvaluation Evaluate(FacilityCategory category, IReadOnlyList<HourlyValue> values, IReadOnlyDictionary<Guid, string> zoneNames, double co2Limit)
        {
            var findings = new List<AuditFinding>();

            var temperatures = values.Where(v => v.Metric == SensorMetric.Temperature).Select(v => (Value: v, Local: SchoolCalendar.ToLocal(v.Hour))).ToList();
            var co2 = values.Where(v => v.Metric == SensorMetric.Co2).ToList();
            var power = values.Where(v => v.Metric == SensorMetric.Power).Select(v => (Value: v, Local: SchoolCalendar.ToLocal(v.Hour))).ToList();

            // Přetápění ve dnech, kdy má být budova v útlumu
            var freeDayTemperatures = temperatures
                .Where(t => SchoolCalendar.IsFreeDay(category, DateOnly.FromDateTime(t.Local)) && t.Local.Hour is >= 8 and < 18)
                .Select(t => t.Value.Value)
                .ToList();

            double? freeDayMean = freeDayTemperatures.Count > 0 ? freeDayTemperatures.Average() : null;
            var overheatLimit = category == FacilityCategory.Social ? 23.5 : 21.5;
            var freeDayOverheating = freeDayMean > overheatLimit && category != FacilityCategory.Health;

            if (freeDayOverheating)
            {
                var freeDayLabel = category == FacilityCategory.Culture ? "v zavíracím dni" : "o víkendu";
                findings.Add(new AuditFinding(
                    "Přetápění mimo provoz",
                    $"Objekt držel {freeDayLabel} průměrně {Format(freeDayMean!.Value)} °C, přestože měl být v útlumu na 18 °C. Pravděpodobně chybný týdenní program ekvitermní regulace.",
                    "Vytápění",
                    AuditStatus.Critical));
            }

            // Noční útlum v provozní dny: v noci se topí (příkon nad základní spotřebou) a zóny drží komfortní teplotu.
            // Samotná teplota nestačí — po teplém dni zůstává budova vyhřátá i bez topení.
            bool OperatingNight(DateTime local) => !SchoolCalendar.IsFreeDay(category, DateOnly.FromDateTime(local)) && local.Hour == 2;

            var nightTemperatures = temperatures.Where(t => OperatingNight(t.Local)).Select(t => t.Value.Value).ToList();
            var nightPower = power.Where(p => OperatingNight(p.Local)).Select(p => p.Value.Value).ToList();
            var basePower = power.Count > 0 ? power.Min(p => p.Value.Value) : 0;

            double? nightMean = nightTemperatures.Count > 0 ? nightTemperatures.Average() : null;
            var nightHeating = nightPower.Count > 0 && nightPower.Average() > 2 * basePower && nightPower.Average() - basePower > 5;
            var nightLimit = category is FacilityCategory.Social or FacilityCategory.Health ? 21.5 : 20.5;
            var nightSetbackMissing = nightHeating && nightMean >= nightLimit && category != FacilityCategory.Culture;

            if (nightSetbackMissing)
            {
                findings.Add(new AuditFinding(
                    "Nefunguje noční útlum",
                    $"Ve 2 hodiny ráno se topí a objekt drží průměrně {Format(nightMean!.Value)} °C. Noční útlum o 2–3 °C ušetří bez investice několik procent tepla.",
                    "Regulace",
                    AuditStatus.Warning));
            }

            // Ranní náběh — příkon mezi 4. a 5. hodinou výrazně nad nočním minimem
            if (category == FacilityCategory.School)
            {
                var early = power.Where(p => !SchoolCalendar.IsFreeDay(category, DateOnly.FromDateTime(p.Local)) && p.Local.Hour == 4).Select(p => p.Value.Value).ToList();
                var night = power.Where(p => !SchoolCalendar.IsFreeDay(category, DateOnly.FromDateTime(p.Local)) && p.Local.Hour == 2).Select(p => p.Value.Value).ToList();

                if (early.Count > 0 && night.Count > 0 && early.Average() > 2 * Math.Max(1, night.Average()))
                {
                    findings.Add(new AuditFinding(
                        "Ranní náběh vytápění příliš brzy",
                        "Vytápění se rozbíhá už před 4. hodinou, přestože výuka začíná v 8:00. Posunutí náběhu v časovém programu je úspora bez investice.",
                        "Regulace",
                        AuditStatus.Nominal));
                }
            }

            // CO₂ — hodiny, kdy aspoň jedna zóna překročila limit
            var hoursOverLimit = co2
                .GroupBy(v => v.Hour)
                .Count(g => g.Max(v => v.Value) > co2Limit);

            if (hoursOverLimit >= 2)
            {
                var worst = co2
                    .GroupBy(v => v.ZoneId)
                    .Select(g => (ZoneId: g.Key, Hours: g.Count(v => v.Value > co2Limit), Peak: g.Max(v => v.Value)))
                    .OrderByDescending(z => z.Hours)
                    .First();

                var zoneName = worst.ZoneId != null && zoneNames.TryGetValue(worst.ZoneId.Value, out var name) ? name : "jedné ze zón";

                findings.Add(new AuditFinding(
                    "Zvýšené CO₂",
                    $"Za posledních 7 dní bylo CO₂ {hoursOverLimit} h nad limitem {co2Limit:0} ppm, nejvíc v zóně {zoneName} (špička {worst.Peak:0} ppm). Nad ~1 000 ppm klesá soustředění — větrání o přestávkách nebo seřízení rekuperace.",
                    "Vnitřní prostředí",
                    AuditStatus.Warning));
            }

            // Aktuální hodnoty — poslední hodina s daty
            var lastHour = values.Count > 0 ? values.Max(v => v.Hour) : (DateTimeOffset?)null;
            var latest = lastHour == null ? [] : values.Where(v => v.Hour == lastHour).ToList();

            var status = findings.Count == 0 ? AuditStatus.Nominal : findings.Max(f => f.Severity);
            var headline = findings.OrderByDescending(f => f.Severity).FirstOrDefault()?.Title ?? "Optimální chod";
            var summary = findings.Count == 0
                ? "Objekt drží útlum mimo provozní hodiny a CO₂ zůstává pod limitem. Spotřeba odpovídá provoznímu kalendáři — doporučeno ponechat stávající nastavení."
                : string.Join(" ", findings.OrderByDescending(f => f.Severity).Select(f => f.Detail));

            return new AuditEvaluation(
                status,
                headline,
                summary,
                findings,
                Average(latest.Where(v => v.Metric == SensorMetric.Temperature)),
                latest.Where(v => v.Metric == SensorMetric.Co2).Select(v => (double?)Math.Round(v.Value)).Max(),
                Average(latest.Where(v => v.Metric == SensorMetric.Power)),
                hoursOverLimit,
                freeDayMean,
                nightMean,
                power.Sum(p => p.Value.Value),
                findings.Any(f => f.Tag is "Vytápění" or "Regulace"),
                freeDayOverheating,
                nightSetbackMissing);
        }

        private static double? Average(IEnumerable<HourlyValue> values)
        {
            var list = values.Select(v => v.Value).ToList();

            return list.Count == 0 ? null : Math.Round(list.Average(), 1);
        }

        private static string Format(double value) => value.ToString("0.0", Czech);
    }
}
