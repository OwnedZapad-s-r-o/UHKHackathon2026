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
            // a pro jednotlivé budovy se měření vyberou přes senzory těch budov (index SensorId + Timestamp).
            // Místnost a veličinu, kterou měří skutečné čidlo, počítá audit jen ze skutečných měření — starší simulace
            // té místnosti by jinak ztrátu držela ještě týden po připojení čidla.
            var rows = await (ids == null
                ? _dbContext.Database.SqlQuery<HourlyRow>($"""
                    SELECT s."FacilityId", s."ZoneId", s."Metric", date_trunc('hour', r."Timestamp") AS "Hour", avg(r."Value") AS "Value"
                    FROM "SensorReadings" r
                    JOIN "Sensors" s ON s."Id" = r."SensorId"
                    WHERE r."Timestamp" >= {from}
                      AND NOT EXISTS (
                        SELECT 1 FROM "Sensors" p
                        WHERE s."Source" = 'Simulated' AND p."Source" = 'Physical' AND p."IsActive"
                          AND p."FacilityId" = s."FacilityId" AND p."ZoneId" IS NOT DISTINCT FROM s."ZoneId" AND p."Metric" = s."Metric")
                    GROUP BY s."FacilityId", s."ZoneId", s."Metric", date_trunc('hour', r."Timestamp")
                    """)
                : _dbContext.Database.SqlQuery<HourlyRow>($"""
                    SELECT s."FacilityId", s."ZoneId", s."Metric", date_trunc('hour', r."Timestamp") AS "Hour", avg(r."Value") AS "Value"
                    FROM "Sensors" s
                    JOIN "SensorReadings" r ON r."SensorId" = s."Id" AND r."Timestamp" >= {from}
                    WHERE s."FacilityId" = ANY({ids})
                      AND NOT EXISTS (
                        SELECT 1 FROM "Sensors" p
                        WHERE s."Source" = 'Simulated' AND p."Source" = 'Physical' AND p."IsActive"
                          AND p."FacilityId" = s."FacilityId" AND p."ZoneId" IS NOT DISTINCT FROM s."ZoneId" AND p."Metric" = s."Metric")
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

            var heatPrices = await HeatPricesAsync(facilities, ct);

            // Přilehlé budovy doplněné správcem (katastr je k objektu nevede) zvětšují vytápěnou plochu
            var addedAreas = await _dbContext.FacilityBuildingParts
                .AsNoTracking()
                .Where(p => ids == null || ids.Contains(p.FacilityId))
                .GroupBy(p => p.FacilityId)
                .Select(g => new { FacilityId = g.Key, Area = g.Sum(p => p.AreaSquareMeters) })
                .ToDictionaryAsync(g => g.FacilityId, g => g.Area, ct);

            // Budovy, kde teplotu měří aspoň jedno skutečné čidlo — ostatním se zapomene ztráta „před čidlem“
            var measured = (await _dbContext.Sensors
                .AsNoTracking()
                .Where(s => s.Source == SensorSource.Physical && s.IsActive && s.Metric == SensorMetric.Temperature && (ids == null || ids.Contains(s.FacilityId)))
                .Select(s => s.FacilityId)
                .Distinct()
                .ToListAsync(ct))
                .ToHashSet();

            foreach (var facility in facilities)
            {
                if (facility.Assessment?.SensorsSince != null && !measured.Contains(facility.Id))
                    facility.Assessment.ClearSensorBaseline();

                if (!byFacility.TryGetValue(facility.Id, out var facilityRows))
                    continue;

                var values = facilityRows
                    .Select(r => new HourlyValue(r.ZoneId, Enum.Parse<SensorMetric>(r.Metric), r.Hour, r.Value))
                    .ToList();

                var zoneNames = facility.Zones.ToDictionary(z => z.Id, z => z.Name);
                var evaluation = FacilityStatusEvaluator.Evaluate(facility.Category, values, zoneNames, _settings.Co2LimitPpm);

                var heatPrice = facility.OrganizationId is { } organizationId && heatPrices.TryGetValue(organizationId, out var contractPrice) ? contractPrice : _settings.HeatPriceCzkPerKwh;
                var (annualLoss, savingPercent) = EstimateLoss(facility, values, evaluation, heatPrice, addedAreas.GetValueOrDefault(facility.Id));

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

        // Cena tepla ze smluv organizace v Registru smluv (dodávka tepla, jinak plynu) — organizace bez ceny ve smlouvě
        // v seznamu nejsou a počítá se s paušální cenou z AuditSettings
        private async Task<Dictionary<Guid, decimal>> HeatPricesAsync(IReadOnlyCollection<Facility> facilities, CancellationToken ct)
        {
            var organizationIds = facilities.Where(f => f.OrganizationId != null).Select(f => f.OrganizationId!.Value).Distinct().ToList();

            var contracts = await _dbContext.FacilityContracts
                .AsNoTracking()
                .Where(c => organizationIds.Contains(c.OrganizationId) && c.Category == ContractCategory.EnergySupply && c.UnitPriceCzkPerMwh != null)
                .Select(c => new { c.OrganizationId, c.Id, c.Commodity, c.UnitPriceCzkPerMwh, c.DateSigned })
                .ToListAsync(ct);

            return contracts
                .GroupBy(c => c.OrganizationId)
                .Select(g => (OrganizationId: g.Key, Price: EnergyPriceExtractor.SelectHeatPrice(g.Select(c => new ContractPrice(c.Id, c.Commodity, c.UnitPriceCzkPerMwh, c.DateSigned)), _settings.HeatPriceCzkPerKwh)))
                .Where(p => p.Price.FromContract)
                .ToDictionary(p => p.OrganizationId, p => p.Price.CzkPerKwh);
        }

        // Ztráta z naměřené vnitřní teploty posledních 7 dní (skutečná čidla i simulovaná tam, kde čidlo zatím není):
        // hodiny přetápění mimo provoz × tepelná ztráta budovy (plocha a stáří z ČÚZK) → kWh za týden
        // × počet týdnů topné sezóny × cena tepla (ze smlouvy, jinak paušál). Když skutečné čidlo ukáže útlum, ztráta klesne na nulu.
        // addedArea = zastavěná plocha přilehlých budov, které doplnil správce (přičte se k ploše z ČÚZK)
        private (decimal AnnualLoss, double SavingPercent) EstimateLoss(Facility facility, IReadOnlyList<HourlyValue> values, AuditEvaluation evaluation, decimal heatPriceCzkPerKwh, double addedArea)
        {
            // Opravy od správce (plocha, podlaží, rok výstavby) mají přednost před ČÚZK
            var builtUpArea = facility.Building?.EffectiveArea(addedArea) ?? (addedArea > 0 ? addedArea : null);
            var floors = facility.Building?.EffectiveFloors ?? BuildingEstimator.EstimateFloors(builtUpArea, facility.Category);
            var floorArea = BuildingEstimator.FloorArea(builtUpArea, floors, facility.Category);
            var heatLoss = floorArea * BuildingEstimator.SpecificHeatLoss(facility.Building?.EffectiveEnergyCategory ?? EnergyCategory.Unknown);

            var loss = HeatingLossEstimator.Estimate(facility.Category, values, heatLoss, evaluation.FreeDayOverheating, evaluation.NightSetbackMissing);
            if (loss.WeeklyExcessKwh <= 0)
                return (0m, 0);

            // Zaokrouhleno na stovky Kč — víc přesnosti odhad nemá
            var annualLoss = Math.Round((decimal)loss.WeeklyExcessKwh * _settings.HeatingSeasonWeeks * heatPriceCzkPerKwh / 100) * 100;

            // Podíl na spotřebě průměrného týdne topné sezóny (vnitřek 20 °C, venku průměr sezóny)
            var seasonWeekKwh = heatLoss * (20 - _settings.HeatingSeasonMeanOutdoor) * 168 / 1000.0;
            var savingPercent = seasonWeekKwh > 0 ? Math.Min(60, Math.Round(loss.WeeklyExcessKwh / seasonWeekKwh * 100, 1)) : 0;

            return (annualLoss, savingPercent);
        }
    }
}
