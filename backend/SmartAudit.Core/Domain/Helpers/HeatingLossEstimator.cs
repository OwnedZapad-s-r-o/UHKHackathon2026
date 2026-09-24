using SmartAudit.Core.Domain.Enums;

namespace SmartAudit.Core.Domain.Helpers
{
    public record HeatingLoss(double WeeklyExcessKwh, double FreeDayExcessKwh, double NightExcessKwh, int MeasuredHours);

    // Ztráta tepla z naměřené vnitřní teploty — ne ze simulačního profilu budovy.
    // Budova drží teplotu T místo útlumové T_útlum → navíc uteče H · (T − T_útlum) wattů, kde H = podlahová plocha × měrná ztráta
    // podle stáří budovy (ČÚZK). Venkovní teplota se odečte: Q(T) − Q(T_útlum) = H · (T − T_útlum).
    // Sčítají se jen hodiny, kde audit zjištění opravdu našel (přetápění ve volných dnech, chybějící noční útlum),
    // aby se do ztráty nepočítalo přirozené dochlazování budovy po provozu.
    public static class HeatingLossEstimator
    {
        // Útlumová teplota mimo provoz: školy a kultura 18 °C, sociální služby a nemocnice (lidé v budově i v noci) 20 °C
        public static double SetbackTemperature(FacilityCategory category) => category is FacilityCategory.Social or FacilityCategory.Health ? 20.0 : 18.0;

        // Noční hodiny provozního dne, kdy má běžet útlum
        private static bool IsNight(int hour) => hour is >= 22 or < 5;

        // Méně než den měření nestačí na přepočet týdne
        private const int MinimumHours = 24;
        private const int HoursPerWeek = 168;

        public static HeatingLoss Estimate(
            FacilityCategory category,
            IReadOnlyList<HourlyValue> values,
            double heatLossWattsPerKelvin,
            bool freeDayOverheating,
            bool nightSetbackMissing)
        {
            // Průměr zón za každou hodinu — budova jako celek
            var hourly = values
                .Where(v => v.Metric == SensorMetric.Temperature)
                .GroupBy(v => v.Hour)
                .Select(g => (Local: SchoolCalendar.ToLocal(g.Key), Temperature: g.Average(v => v.Value)))
                .ToList();

            if (hourly.Count < MinimumHours || (!freeDayOverheating && !nightSetbackMissing))
                return new HeatingLoss(0, 0, 0, hourly.Count);

            var setback = SetbackTemperature(category);
            double freeDayKwh = 0, nightKwh = 0;

            foreach (var (local, temperature) in hourly)
            {
                var excessKwh = heatLossWattsPerKelvin * Math.Max(0, temperature - setback) / 1000.0;
                var freeDay = SchoolCalendar.IsFreeDay(category, DateOnly.FromDateTime(local));

                if (freeDay && freeDayOverheating)
                    freeDayKwh += excessKwh;
                else if (!freeDay && nightSetbackMissing && IsNight(local.Hour))
                    nightKwh += excessKwh;
            }

            // Kratší okno měření (nové čidlo, výpadek) se přepočte na celý týden
            var scale = (double)HoursPerWeek / Math.Min(HoursPerWeek, hourly.Count);

            return new HeatingLoss((freeDayKwh + nightKwh) * scale, freeDayKwh * scale, nightKwh * scale, hourly.Count);
        }
    }
}
