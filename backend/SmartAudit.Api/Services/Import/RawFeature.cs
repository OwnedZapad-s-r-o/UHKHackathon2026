using System.Globalization;
using System.Text.Json;
using NetTopologySuite.Geometries;
using SmartAudit.Core.Domain.Helpers;

namespace SmartAudit.Api.Services.Import
{
    // Jeden záznam surové vrstvy připravený ke čtení — atributy + geometrie
    public record RawFeature(JsonElement Properties, Geometry? Geometry)
    {
        public string? Text(string name)
        {
            if (!Properties.TryGetProperty(name, out var value))
                return null;

            return value.ValueKind switch
            {
                JsonValueKind.String => KeyNormalizer.Clean(value.GetString()),
                JsonValueKind.Number => value.GetRawText(),
                _ => null,
            };
        }

        public int? Int(string name)
        {
            if (!Properties.TryGetProperty(name, out var value))
                return null;

            if (value.ValueKind == JsonValueKind.Number)
                return (int)Math.Round(value.GetDouble());

            return int.TryParse(Text(name), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;
        }

        public decimal? Decimal(string name)
        {
            if (!Properties.TryGetProperty(name, out var value))
                return null;

            if (value.ValueKind == JsonValueKind.Number)
                return value.GetDecimal();

            return decimal.TryParse(Text(name), NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;
        }

        // Bod objektu — geometrie, jinak atributy x/y (WGS84)
        public Point? Point(GeometryFactory factory)
        {
            if (Geometry is Point point && !point.IsEmpty)
                return factory.CreatePoint(new Coordinate(point.X, point.Y));

            var x = Properties.TryGetProperty("x", out var xValue) && xValue.ValueKind == JsonValueKind.Number ? xValue.GetDouble() : (double?)null;
            var y = Properties.TryGetProperty("y", out var yValue) && yValue.ValueKind == JsonValueKind.Number ? yValue.GetDouble() : (double?)null;

            return x != null && y != null ? factory.CreatePoint(new Coordinate(x.Value, y.Value)) : null;
        }

        public bool Is(string name, string expected)
        {
            return string.Equals(Text(name), expected, StringComparison.OrdinalIgnoreCase);
        }
    }
}
