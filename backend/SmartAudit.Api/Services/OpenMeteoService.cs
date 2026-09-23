using System.Globalization;
using System.Text.Json;

namespace SmartAudit.Api.Services
{
    public record WeatherPoint(string Key, double Latitude, double Longitude);

    public record HourlyWeather(string Key, DateTimeOffset Timestamp, double? Temperature, double? Pm10, double? Pm25);

    public interface IOpenMeteoService
    {
        Task<IReadOnlyList<HourlyWeather>> GetTemperatureAsync(IReadOnlyList<WeatherPoint> points, int pastDays, CancellationToken ct);
        Task<IReadOnlyList<HourlyWeather>> GetAirQualityAsync(IReadOnlyList<WeatherPoint> points, int pastDays, CancellationToken ct);
    }
    public class OpenMeteoService : IOpenMeteoService
    {
        private const string ForecastUrl = "https://api.open-meteo.com/v1/forecast";
        private const string AirQualityUrl = "https://air-quality-api.open-meteo.com/v1/air-quality";

        private readonly HttpClient _httpClient;

        public OpenMeteoService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public Task<IReadOnlyList<HourlyWeather>> GetTemperatureAsync(IReadOnlyList<WeatherPoint> points, int pastDays, CancellationToken ct)
        {
            return FetchAsync(ForecastUrl, "temperature_2m", points, pastDays, ct);
        }

        public Task<IReadOnlyList<HourlyWeather>> GetAirQualityAsync(IReadOnlyList<WeatherPoint> points, int pastDays, CancellationToken ct)
        {
            return FetchAsync(AirQualityUrl, "pm10,pm2_5", points, pastDays, ct);
        }

        // Open-Meteo umí víc lokalit v jednom dotazu — pro 15 ORP stačí jedno volání
        private async Task<IReadOnlyList<HourlyWeather>> FetchAsync(string baseUrl, string variables, IReadOnlyList<WeatherPoint> points, int pastDays, CancellationToken ct)
        {
            if (points.Count == 0)
                return [];

            var latitudes = string.Join(',', points.Select(p => p.Latitude.ToString("0.####", CultureInfo.InvariantCulture)));
            var longitudes = string.Join(',', points.Select(p => p.Longitude.ToString("0.####", CultureInfo.InvariantCulture)));
            var url = $"{baseUrl}?latitude={latitudes}&longitude={longitudes}&hourly={variables}&past_days={pastDays}&forecast_days=2&timezone=GMT";

            using var response = await _httpClient.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));

            var locations = document.RootElement.ValueKind == JsonValueKind.Array
                ? document.RootElement.EnumerateArray().ToList()
                : [document.RootElement];

            var result = new List<HourlyWeather>();

            for (var i = 0; i < Math.Min(points.Count, locations.Count); i++)
            {
                var hourly = locations[i].GetProperty("hourly");
                var times = hourly.GetProperty("time").EnumerateArray().Select(t => t.GetString()!).ToList();

                for (var h = 0; h < times.Count; h++)
                {
                    var timestamp = new DateTimeOffset(DateTime.ParseExact(times[h], "yyyy-MM-dd'T'HH:mm", CultureInfo.InvariantCulture), TimeSpan.Zero);

                    result.Add(new HourlyWeather(
                        points[i].Key,
                        timestamp,
                        Value(hourly, "temperature_2m", h),
                        Value(hourly, "pm10", h),
                        Value(hourly, "pm2_5", h)));
                }
            }

            return result;
        }

        private static double? Value(JsonElement hourly, string variable, int index)
        {
            if (!hourly.TryGetProperty(variable, out var series))
                return null;

            var value = series[index];

            return value.ValueKind == JsonValueKind.Number ? value.GetDouble() : null;
        }
    }
}
