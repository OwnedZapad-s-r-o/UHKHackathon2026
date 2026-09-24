using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using SmartAudit.Core.Data;
using SmartAudit.Core.Domain.Enums;
using SmartAudit.Core.Domain.Helpers;
using SmartAudit.Core.Domain.Settings;

namespace SmartAudit.Api.Services.Simulation
{
    // Simulovaná telemetrie; skutečná čidla chodí přes IQRF gateway (Services/Sensors)
    public interface ISensorDataSource
    {
        Task<int> SyncAsync(DateTimeOffset now, CancellationToken ct);
    }

    public class SimulatedSensorDataSource : ISensorDataSource
    {
        private readonly SmartAuditDbContext _dbContext;
        private readonly IWeatherService _weatherService;
        private readonly SimulationSettings _settings;
        private readonly ILogger<SimulatedSensorDataSource> _logger;

        public SimulatedSensorDataSource(SmartAuditDbContext dbContext, IWeatherService weatherService, SimulationSettings settings, ILogger<SimulatedSensorDataSource> logger)
        {
            _dbContext = dbContext;
            _weatherService = weatherService;
            _settings = settings;
            _logger = logger;
        }

        public async Task<int> SyncAsync(DateTimeOffset now, CancellationToken ct)
        {
            var step = TimeSpan.FromMinutes(_settings.StepMinutes);
            var end = new DateTimeOffset(now.UtcTicks - now.UtcTicks % step.Ticks, TimeSpan.Zero);
            var backfillStart = end.AddDays(-_settings.BackfillDays);

            // Jen aktivní simulovaná čidla — skutečná posílají data sama a zónu, kterou převzala, už simulace nekreslí
            var sensors = await _dbContext.Sensors
                .AsNoTracking()
                .Where(s => s.Source == SensorSource.Simulated && s.IsActive)
                .Select(s => new
                {
                    s.Id,
                    s.Metric,
                    s.Seed,
                    ZoneType = s.Zone != null ? s.Zone.Type : (ZoneType?)null,
                    IsHotspot = s.Zone != null && s.Zone.IsHotspot,
                    s.Facility.Category,
                    s.Facility.SimulationProfile,
                    s.Facility.OrpCode,
                    BuiltUpArea = s.Facility.Building != null ? s.Facility.Building.BuiltUpArea : null,
                    Floors = s.Facility.Building != null ? s.Facility.Building.EstimatedFloors : (int?)null,
                    EnergyCategory = s.Facility.Building != null ? s.Facility.Building.EnergyCategory : EnergyCategory.Unknown,
                    Students = s.Facility.Metrics.Where(m => m.Students != null).OrderByDescending(m => m.SchoolYear).Select(m => m.Students).FirstOrDefault(),
                })
                .ToListAsync(ct);

            // Poslední měření každého senzoru jen v okně dopočtu — starší oddíly tabulky se vůbec nečtou
            // (senzor bez měření v okně by stejně začal od backfillStart)
            var lastReadings = await _dbContext.SensorReadings
                .Where(r => r.Timestamp >= backfillStart)
                .GroupBy(r => r.SensorId)
                .Select(g => new { SensorId = g.Key, Last = g.Max(r => r.Timestamp) })
                .ToDictionaryAsync(x => x.SensorId, x => x.Last, ct);

            var lookup = await _weatherService.GetLookupAsync(backfillStart, end, ct);
            var readings = new ConcurrentBag<(Guid SensorId, DateTimeOffset Timestamp, double Value)>();

            Parallel.ForEach(sensors, new ParallelOptions { CancellationToken = ct }, sensor =>
            {
                var from = lastReadings.TryGetValue(sensor.Id, out var last) ? last.ToUniversalTime() + step : backfillStart;
                if (from > end)
                    return;

                var floors = sensor.Floors ?? BuildingEstimator.EstimateFloors(sensor.BuiltUpArea, sensor.Category);
                var spec = new SensorSimulationSpec(
                    sensor.Metric,
                    sensor.Category,
                    sensor.SimulationProfile,
                    sensor.ZoneType,
                    sensor.Seed,
                    BuildingEstimator.FloorArea(sensor.BuiltUpArea, floors, sensor.Category),
                    BuildingEstimator.SpecificHeatLoss(sensor.EnergyCategory),
                    OccupancyFactor(sensor.Category, sensor.Students),
                    sensor.IsHotspot);

                foreach (var (timestamp, value) in SensorSimulator.Generate(spec, from, end, _settings.StepMinutes, lookup.ForOrp(sensor.OrpCode)))
                    readings.Add((sensor.Id, timestamp, value));
            });

            if (readings.IsEmpty)
                return 0;

            await CopyAsync(readings, ct);

            _logger.LogInformation("Simulace: zapsáno {Count} měření do {End:u}", readings.Count, end);

            return readings.Count;
        }

        // Plnější školy = rychlejší nárůst CO₂; heuristika z počtu žáků (sady KHK)
        private static double OccupancyFactor(FacilityCategory category, int? students)
        {
            if (category != FacilityCategory.School || students is not > 0)
                return 1.0;

            return Math.Clamp(0.8 + students.Value / 2000.0, 0.8, 1.25);
        }

        // Miliony řádků — binární COPY je o řád rychlejší než INSERT přes EF
        private async Task CopyAsync(IEnumerable<(Guid SensorId, DateTimeOffset Timestamp, double Value)> readings, CancellationToken ct)
        {
            var connection = (NpgsqlConnection)_dbContext.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync(ct);

            await using var writer = await connection.BeginBinaryImportAsync(
                "COPY \"SensorReadings\" (\"SensorId\", \"Timestamp\", \"Value\") FROM STDIN (FORMAT BINARY)", ct);

            foreach (var (sensorId, timestamp, value) in readings)
            {
                await writer.StartRowAsync(ct);
                await writer.WriteAsync(sensorId, NpgsqlDbType.Uuid, ct);
                await writer.WriteAsync(timestamp, NpgsqlDbType.TimestampTz, ct);
                await writer.WriteAsync(value, NpgsqlDbType.Double, ct);
            }

            await writer.CompleteAsync(ct);
        }
    }
}
