using Microsoft.EntityFrameworkCore;
using SmartAudit.Api.Services.Import;
using SmartAudit.Core.Data;
using SmartAudit.Core.Data.Tables;
using SmartAudit.Core.Domain.Enums;
using SmartAudit.Core.Domain.Helpers;

namespace SmartAudit.Api.Services
{
    public interface IWeatherService
    {
        Task<int> RefreshAsync(int pastDays, CancellationToken ct);
        Task<OutdoorTemperatureLookup> GetLookupAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken ct);
    }
    public class WeatherService : IWeatherService
    {
        public const string TemperatureSourceKey = "open-meteo-pocasi";
        public const string AirQualitySourceKey = "open-meteo-ovzdusi";

        private readonly SmartAuditDbContext _dbContext;
        private readonly IOpenMeteoService _openMeteoService;
        private readonly IOpenDataImportService _importService;
        private readonly ILogger<WeatherService> _logger;

        public WeatherService(SmartAuditDbContext dbContext, IOpenMeteoService openMeteoService, IOpenDataImportService importService, ILogger<WeatherService> logger)
        {
            _dbContext = dbContext;
            _openMeteoService = openMeteoService;
            _importService = importService;
            _logger = logger;
        }

        // Jeden bod na ORP (těžiště obvodu) — rozdíl teplot uvnitř ORP je pro audit zanedbatelný
        public async Task<int> RefreshAsync(int pastDays, CancellationToken ct)
        {
            var orps = await _dbContext.Regions
                .AsNoTracking()
                .Where(r => r.Type == RegionType.Orp && r.Geometry != null)
                .Select(r => new { r.Code, r.Geometry })
                .ToListAsync(ct);

            var points = orps
                .Select(o => new WeatherPoint(o.Code, o.Geometry!.Centroid.Y, o.Geometry.Centroid.X))
                .ToList();

            if (points.Count == 0)
                return 0;

            IReadOnlyList<HourlyWeather> temperatures;
            try
            {
                temperatures = await _openMeteoService.GetTemperatureAsync(points, pastDays, ct);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                _logger.LogWarning(ex, "Open-Meteo nedostupné, simulace použije náhradní průběh teploty");
                await _importService.RecordExternalImportAsync(TemperatureSourceKey, ImportStatus.Failed, 0, ex.Message, ct);

                return 0;
            }

            var airQuality = new Dictionary<(string, DateTimeOffset), HourlyWeather>();
            try
            {
                foreach (var row in await _openMeteoService.GetAirQualityAsync(points, pastDays, ct))
                    airQuality[(row.Key, row.Timestamp)] = row;

                await _importService.RecordExternalImportAsync(AirQualitySourceKey, ImportStatus.Live, airQuality.Count, null, ct);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                _logger.LogWarning(ex, "Open-Meteo Air Quality nedostupné");
                await _importService.RecordExternalImportAsync(AirQualitySourceKey, ImportStatus.Failed, 0, ex.Message, ct);
            }

            var rows = temperatures.Where(t => t.Temperature != null).ToList();
            if (rows.Count == 0)
                return 0;

            var from = rows.Min(r => r.Timestamp);
            var to = rows.Max(r => r.Timestamp);

            await _dbContext.WeatherObservations.Where(w => w.Timestamp >= from && w.Timestamp <= to).ExecuteDeleteAsync(ct);

            foreach (var row in rows)
            {
                var observation = new WeatherObservation(row.Key, row.Timestamp, row.Temperature!.Value);

                if (airQuality.TryGetValue((row.Key, row.Timestamp), out var air))
                    observation.SetAirQuality(air.Pm10, air.Pm25);

                _dbContext.WeatherObservations.Add(observation);
            }

            await _dbContext.SaveChangesAsync(ct);
            _dbContext.ChangeTracker.Clear();

            await _importService.RecordExternalImportAsync(TemperatureSourceKey, ImportStatus.Live, rows.Count, null, ct);

            return rows.Count;
        }

        public async Task<OutdoorTemperatureLookup> GetLookupAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken ct)
        {
            var observations = await _dbContext.WeatherObservations
                .AsNoTracking()
                .Where(w => w.Timestamp >= from.AddHours(-2) && w.Timestamp <= to.AddHours(2))
                .OrderBy(w => w.Timestamp)
                .Select(w => new { w.OrpCode, w.Timestamp, w.Temperature })
                .ToListAsync(ct);

            var series = observations
                .GroupBy(o => o.OrpCode)
                .ToDictionary(
                    g => g.Key,
                    g => (Ticks: g.Select(o => o.Timestamp.UtcTicks).ToArray(), Values: g.Select(o => o.Temperature).ToArray()));

            return new OutdoorTemperatureLookup(series);
        }
    }

    // Hodinová teplota s lineární interpolací; kde data chybí, typický průběh dne
    public class OutdoorTemperatureLookup
    {
        private readonly Dictionary<string, (long[] Ticks, double[] Values)> _series;

        public OutdoorTemperatureLookup(Dictionary<string, (long[] Ticks, double[] Values)> series)
        {
            _series = series;
        }

        public bool HasData => _series.Count > 0;

        public Func<DateTimeOffset, double> ForOrp(string orpCode)
        {
            if (!_series.TryGetValue(orpCode, out var series) || series.Ticks.Length < 2)
                return SensorSimulator.FallbackOutdoorTemperature;

            return time => Interpolate(series.Ticks, series.Values, time);
        }

        private static double Interpolate(long[] ticks, double[] values, DateTimeOffset time)
        {
            var t = time.UtcTicks;

            if (t <= ticks[0] || t >= ticks[^1])
                return SensorSimulator.FallbackOutdoorTemperature(time);

            var index = Array.BinarySearch(ticks, t);
            if (index >= 0)
                return values[index];

            var upper = ~index;
            var lower = upper - 1;
            var ratio = (double)(t - ticks[lower]) / (ticks[upper] - ticks[lower]);

            return values[lower] + (values[upper] - values[lower]) * ratio;
        }
    }
}
