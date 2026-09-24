using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SmartAudit.Core.Data;
using SmartAudit.Core.Data.Tables;
using SmartAudit.Core.Domain.Enums;
using SmartAudit.Core.Domain.Helpers;
using SmartAudit.Core.Domain.Settings;

namespace SmartAudit.Api.Services.Audit
{
    public interface IFacilityAuditService
    {
        // facilityIds = null → celý kraj (nový krok simulace), jinak jen vyjmenované budovy (nová měření z čidel)
        Task<int> EvaluateAsync(IReadOnlyCollection<Guid>? facilityIds, DateTimeOffset now, CancellationToken ct);
    }

    public class FacilityAuditService : IFacilityAuditService
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly SmartAuditDbContext _dbContext;
        private readonly AuditSettings _settings;

        public FacilityAuditService(SmartAuditDbContext dbContext, AuditSettings settings)
        {
            _dbContext = dbContext;
            _settings = settings;
        }

        private sealed class HourlyRow
        {
            public Guid FacilityId { get; set; }
            public Guid? ZoneId { get; set; }
            public string Metric { get; set; } = string.Empty;
            public DateTimeOffset Hour { get; set; }
            public double Value { get; set; }
        }

        public async Task<int> EvaluateAsync(IReadOnlyCollection<Guid>? facilityIds, DateTimeOffset now, CancellationToken ct)
        {
            if (facilityIds is { Count: 0 })
                return 0;

            var from = now.AddDays(-7);
            var ids = facilityIds?.ToArray();

            // Hodinové průměry posledních 7 dní; oddíly tabulky podle měsíce zajistí, že se čtou jen poslední měsíce,
            // a pro jednotlivé budovy se měření vyberou přes senzory těch budov (index SensorId + Timestamp)
            var rows = await (ids == null
                ? _dbContext.Database.SqlQuery<HourlyRow>($"""
                    SELECT s."FacilityId", s."ZoneId", s."Metric", date_trunc('hour', r."Timestamp") AS "Hour", avg(r."Value") AS "Value"
                    FROM "SensorReadings" r
                    JOIN "Sensors" s ON s."Id" = r."SensorId"
                    WHERE r."Timestamp" >= {from}
                    GROUP BY s."FacilityId", s."ZoneId", s."Metric", date_trunc('hour', r."Timestamp")
                    """)
                : _dbContext.Database.SqlQuery<HourlyRow>($"""
                    SELECT s."FacilityId", s."ZoneId", s."Metric", date_trunc('hour', r."Timestamp") AS "Hour", avg(r."Value") AS "Value"
                    FROM "Sensors" s
                    JOIN "SensorReadings" r ON r."SensorId" = s."Id" AND r."Timestamp" >= {from}
                    WHERE s."FacilityId" = ANY({ids})
                    GROUP BY s."FacilityId", s."ZoneId", s."Metric", date_trunc('hour', r."Timestamp")
                    """)).ToListAsync(ct);

            var byFacility = rows.GroupBy(r => r.FacilityId).ToDictionary(g => g.Key, g => g.ToList());

            var facilities = await _dbContext.Facilities
                .Where(f => ids == null || ids.Contains(f.Id))
                .Include(f => f.Assessment)
                .Include(f => f.Building)
                .Include(f => f.Zones)
                .AsSplitQuery()
                .ToListAsync(ct);

            foreach (var facility in facilities)
            {
                if (!byFacility.TryGetValue(facility.Id, out var facilityRows))
                    continue;

                var values = facilityRows
                    .Select(r => new HourlyValue(r.ZoneId, Enum.Parse<SensorMetric>(r.Metric), r.Hour, r.Value))
                    .ToList();

                var zoneNames = facility.Zones.ToDictionary(z => z.Id, z => z.Name);
                var evaluation = FacilityStatusEvaluator.Evaluate(facility.Category, values, zoneNames, _settings.Co2LimitPpm);

                var (annualLoss, savingPercent) = EstimateLoss(facility, values, evaluation);

                var assessment = facility.Assessment;
                if (assessment == null)
                {
                    assessment = new FacilityAssessment(facility.Id);
                    _dbContext.FacilityAssessments.Add(assessment);
                }

                assessment.Update(
                    evaluation.Status,
                    evaluation.Headline,
                    evaluation.Summary,
                    JsonSerializer.Serialize(evaluation.Findings.Select(f => new { f.Title, f.Detail, f.Tag, Severity = f.Severity.ToString() }), JsonOptions),
                    evaluation.LatestTemperature,
                    evaluation.LatestCo2,
                    evaluation.LatestPower,
                    evaluation.Co2HoursOverLimit,
                    evaluation.FreeDayMeanTemperature,
                    evaluation.NightMeanTemperature,
                    Math.Round(evaluation.WeeklyEnergyKwh),
                    annualLoss,
                    savingPercent);
            }

            await _dbContext.SaveChangesAsync(ct);
            _dbContext.ChangeTracker.Clear();

            return byFacility.Count;
        }

        // Ztráta z naměřené vnitřní teploty posledních 7 dní (skutečná čidla i simulovaná tam, kde čidlo zatím není):
        // hodiny přetápění mimo provoz × tepelná ztráta budovy (plocha a stáří z ČÚZK) → kWh za týden
        // × počet týdnů topné sezóny × cena tepla. Když skutečné čidlo ukáže útlum, ztráta klesne na nulu.
        private (decimal AnnualLoss, double SavingPercent) EstimateLoss(Facility facility, IReadOnlyList<HourlyValue> values, AuditEvaluation evaluation)
        {
            var floors = facility.Building?.EstimatedFloors ?? BuildingEstimator.EstimateFloors(facility.Building?.BuiltUpArea, facility.Category);
            var floorArea = BuildingEstimator.FloorArea(facility.Building?.BuiltUpArea, floors, facility.Category);
            var heatLoss = floorArea * BuildingEstimator.SpecificHeatLoss(facility.Building?.EnergyCategory ?? EnergyCategory.Unknown);

            var loss = HeatingLossEstimator.Estimate(facility.Category, values, heatLoss, evaluation.FreeDayOverheating, evaluation.NightSetbackMissing);
            if (loss.WeeklyExcessKwh <= 0)
                return (0m, 0);

            // Zaokrouhleno na stovky Kč — víc přesnosti odhad nemá
            var annualLoss = Math.Round((decimal)loss.WeeklyExcessKwh * _settings.HeatingSeasonWeeks * _settings.HeatPriceCzkPerKwh / 100) * 100;

            // Podíl na spotřebě průměrného týdne topné sezóny (vnitřek 20 °C, venku průměr sezóny)
            var seasonWeekKwh = heatLoss * (20 - _settings.HeatingSeasonMeanOutdoor) * 168 / 1000.0;
            var savingPercent = seasonWeekKwh > 0 ? Math.Min(60, Math.Round(loss.WeeklyExcessKwh / seasonWeekKwh * 100, 1)) : 0;

            return (annualLoss, savingPercent);
        }
    }
}
