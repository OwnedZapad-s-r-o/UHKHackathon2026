using System.Globalization;
using System.Text;

namespace SmartAudit.Api.Features.OpenData.GetOpenFacilities
{
    public record OpenFeatureCollectionDto(string Type, IReadOnlyCollection<OpenFeatureDto> Features);

    public record OpenFeatureDto(string Type, OpenPointDto Geometry, OpenFacilityDto Properties);

    // GeoJSON souřadnice jsou [zeměpisná délka, šířka] v WGS 84
    public record OpenPointDto(string Type, double[] Coordinates);

    // Převody seznamu objektů do formátů otevřených dat
    public static class OpenFacilitiesFormats
    {
        private static readonly string[] CsvHeader =
        [
            "id", "kod", "nazev", "kategorie", "druh", "ico", "ulice", "cislo_domu", "psc",
            "obec_kod", "obec", "orp_kod", "orp", "okres_kod", "okres", "lat", "lon",
            "pamatkova_ochrana", "ma_pudorys", "stav", "zjisteni", "teplota_c", "co2_ppm", "prikon_kw", "co2_hodin_nad_limitem_7d", "zive_cidlo", "vyhodnoceno",
            "rezim_dat", "predaudit_skore", "predaudit_stav", "predaudit_hlavni_pricina", "merna_ztrata_w_m2k", "denostupne", "potreba_tepla_kwh_rok",
            "iqrf_cidla", "iqrf_cena_sady_czk",
        ];

        public static OpenFeatureCollectionDto ToGeoJson(IReadOnlyCollection<OpenFacilityDto> facilities)
        {
            var features = facilities
                .Select(f => new OpenFeatureDto("Feature", new OpenPointDto("Point", [f.Longitude, f.Latitude]), f))
                .ToList();

            return new OpenFeatureCollectionDto("FeatureCollection", features);
        }

        // CSV podle RFC 4180 (čárka, tečka v číslech, UTF-8 s BOM, aby ho Excel otevřel s diakritikou)
        public static byte[] ToCsv(IReadOnlyCollection<OpenFacilityDto> facilities)
        {
            var csv = new StringBuilder();
            csv.AppendJoin(',', CsvHeader).Append("\r\n");

            foreach (var f in facilities)
            {
                string?[] row =
                [
                    f.Id.ToString(), f.Code, f.Name, f.Category.ToString(), f.Kind, f.Ico, f.Street, f.HouseNumber, f.Psc,
                    f.MunicipalityCode, f.MunicipalityName, f.OrpCode, f.OrpName, f.DistrictCode, f.DistrictName,
                    Number(f.Latitude), Number(f.Longitude),
                    f.HeritageStatus.ToString(), f.HasFootprint ? "true" : "false", f.Status.ToString(), f.Headline,
                    Number(f.Temperature), Number(f.Co2), Number(f.Power), Number(f.Co2HoursOverLimit), f.HasLiveSensors ? "true" : "false",
                    f.EvaluatedAt?.ToString("O", CultureInfo.InvariantCulture),
                    f.DataMode, f.PreAuditScore?.ToString(CultureInfo.InvariantCulture), f.PreAuditStatus?.ToString(), f.PreAuditDriver,
                    Number(f.SpecificHeatLoss), Number(f.DegreeDays), Number(f.AnnualHeatDemandKwh),
                    f.IqrfSensors?.ToString(CultureInfo.InvariantCulture), f.IqrfHardwareCostCzk?.ToString("0", CultureInfo.InvariantCulture),
                ];

                csv.AppendJoin(',', row.Select(Escape)).Append("\r\n");
            }

            return [.. Encoding.UTF8.GetPreamble(), .. Encoding.UTF8.GetBytes(csv.ToString())];
        }

        private static string? Number(double? value) => value?.ToString("0.######", CultureInfo.InvariantCulture);

        private static string Escape(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return value.IndexOfAny([',', '"', '\r', '\n']) >= 0 ? $"\"{value.Replace("\"", "\"\"")}\"" : value;
        }
    }
}
