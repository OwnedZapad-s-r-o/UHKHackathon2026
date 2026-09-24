using SmartAudit.Core.Domain.Enums;
using SmartAudit.Core.Domain.Helpers;

namespace SmartAudit.Tests
{
    // Ztráta v Kč se počítá z naměřené teploty, ne ze simulačního profilu budovy (PLAN-ARCH-01):
    // škola, která o víkendu drží útlum, má nulovou ztrátu, i kdyby ji simulace vedla jako „přetápějící“.
    public class HeatingLossEstimatorTests
    {
        // Běžný školní týden bez svátků a prázdnin (pondělí 9. 11. 2026)
        private static readonly DateOnly Monday = new(2026, 11, 9);

        // Tepelná ztráta budovy: 3 000 m² × 1,5 W/(m²·K)
        private const double HeatLossWattsPerKelvin = 4500;

        [Fact]
        public void Weekend_in_setback_has_no_loss()
        {
            var values = Week(weekday: 21.0, weekend: 18.0);

            var evaluation = FacilityStatusEvaluator.Evaluate(FacilityCategory.School, values, new Dictionary<Guid, string>(), 1500);
            var loss = HeatingLossEstimator.Estimate(FacilityCategory.School, values, HeatLossWattsPerKelvin, evaluation.FreeDayOverheating, evaluation.NightSetbackMissing);

            Assert.False(evaluation.FreeDayOverheating);
            Assert.Equal(0, loss.WeeklyExcessKwh);
        }

        [Fact]
        public void Weekend_overheating_counts_weekend_hours_above_setback()
        {
            var values = Week(weekday: 21.0, weekend: 23.0);

            var evaluation = FacilityStatusEvaluator.Evaluate(FacilityCategory.School, values, new Dictionary<Guid, string>(), 1500);
            var loss = HeatingLossEstimator.Estimate(FacilityCategory.School, values, HeatLossWattsPerKelvin, evaluation.FreeDayOverheating, evaluation.NightSetbackMissing);

            Assert.True(evaluation.FreeDayOverheating);

            // 48 víkendových hodin × (23 − 18) K × 4 500 W/K = 1 080 kWh; pracovní dny se nepočítají
            Assert.Equal(1080, loss.FreeDayExcessKwh, precision: 6);
            Assert.Equal(0, loss.NightExcessKwh);
            Assert.Equal(1080, loss.WeeklyExcessKwh, precision: 6);
        }

        [Fact]
        public void Too_few_measured_hours_are_not_extrapolated()
        {
            var values = Week(weekday: 21.0, weekend: 23.0).Take(12).ToList();

            var loss = HeatingLossEstimator.Estimate(FacilityCategory.School, values, HeatLossWattsPerKelvin, freeDayOverheating: true, nightSetbackMissing: false);

            Assert.Equal(0, loss.WeeklyExcessKwh);
        }

        // Hodinové teploty jedné zóny po celý týden (sobota a neděle = víkend)
        private static List<HourlyValue> Week(double weekday, double weekend)
        {
            var zone = Guid.NewGuid();
            var start = SchoolCalendar.LocalMidnightUtc(Monday);

            return Enumerable.Range(0, 7 * 24)
                .Select(h => start.AddHours(h))
                .Select(hour =>
                {
                    var day = DateOnly.FromDateTime(SchoolCalendar.ToLocal(hour));
                    return new HourlyValue(zone, SensorMetric.Temperature, hour, SchoolCalendar.IsWeekend(day) ? weekend : weekday);
                })
                .ToList();
        }
    }
}
