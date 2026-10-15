using System.Globalization;
using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAudit.Core.Data;
using SmartAudit.Core.Domain;
using SmartAudit.Core.Domain.Enums;

namespace SmartAudit.Api.Features.OpenData.GetOpenPreAudits
{
    public record GetOpenPreAuditsCommand(FacilityCategory? Category = null, string? District = null) : IRequest<Response<IReadOnlyCollection<OpenPreAuditDto>>>;

    // Pre-audit objektu jako otevřená data — skóre, pět parametrů s vysvětlením a klíči zdrojových sad, fyzika budovy
    // a velikost sady IQRF. Bez částek úspor (stejně jako zbytek otevřeného API); cena sady je ceníková.
    public record OpenPreAuditDto(
        Guid FacilityId,
        string Code,
        string Name,
        FacilityCategory Category,
        string DistrictCode,
        string OrpCode,
        double Latitude,
        double Longitude,
        int Score,
        AuditStatus Status,
        string DominantDriver,
        IReadOnlyCollection<OpenPreAuditParameterDto> Parameters,
        double SpecificHeatLoss,
        double HeatLossWattsPerKelvin,
        double DegreeDays,
        double AnnualHeatDemandKwh,
        bool HeatPriceFromContract,
        double RegulationSavingPercent,
        int IqrfGateways,
        int IqrfSensors,
        decimal IqrfHardwareCostCzk,
        DateTimeOffset ComputedAt);

    public record OpenPreAuditParameterDto(string Key, string Name, int Score, int MaxScore, string Value, string Explanation, string[] Sources);

    public class GetOpenPreAuditsHandler : IRequestHandler<GetOpenPreAuditsCommand, Response<IReadOnlyCollection<OpenPreAuditDto>>>
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly SmartAuditDbContext _dbContext;

        public GetOpenPreAuditsHandler(SmartAuditDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Response<IReadOnlyCollection<OpenPreAuditDto>>> Handle(GetOpenPreAuditsCommand request, CancellationToken ct)
        {
            var query = _dbContext.FacilityPreAudits.AsNoTracking();

            if (request.Category != null)
                query = query.Where(p => p.Facility.Category == request.Category);

            if (!string.IsNullOrWhiteSpace(request.District))
                query = query.Where(p => p.Facility.DistrictCode == request.District);

            var rows = await query
                .OrderBy(p => p.Facility.Code)
                .Select(p => new
                {
                    p.FacilityId,
                    p.Facility.Code,
                    p.Facility.Name,
                    p.Facility.Category,
                    p.Facility.DistrictCode,
                    p.Facility.OrpCode,
                    Latitude = p.Facility.Location.Y,
                    Longitude = p.Facility.Location.X,
                    p.Score,
                    p.Status,
                    p.DominantDriver,
                    p.ParametersJson,
                    p.SpecificHeatLoss,
                    p.HeatLossWattsPerKelvin,
                    p.DegreeDays,
                    p.AnnualHeatDemandKwh,
                    p.HeatPriceFromContract,
                    p.RegulationSavingPercent,
                    p.IqrfGateways,
                    p.IqrfSensors,
                    p.IqrfHardwareCostCzk,
                    p.ComputedAt,
                })
                .ToListAsync(ct);

            var result = rows
                .Select(r => new OpenPreAuditDto(
                    r.FacilityId, r.Code, r.Name, r.Category, r.DistrictCode, r.OrpCode, r.Latitude, r.Longitude,
                    r.Score, r.Status, r.DominantDriver,
                    JsonSerializer.Deserialize<List<OpenPreAuditParameterDto>>(r.ParametersJson, JsonOptions) ?? [],
                    r.SpecificHeatLoss, r.HeatLossWattsPerKelvin, r.DegreeDays, r.AnnualHeatDemandKwh, r.HeatPriceFromContract,
                    r.RegulationSavingPercent, r.IqrfGateways, r.IqrfSensors, r.IqrfHardwareCostCzk, r.ComputedAt))
                .ToList();

            return Response.Success<IReadOnlyCollection<OpenPreAuditDto>>(result);
        }
    }

    public static class OpenPreAuditFormats
    {
        private static readonly string[] Parameters = ["A", "B", "C", "D", "E"];

        // CSV (RFC 4180, UTF-8 s BOM) — jeden řádek na objekt, body parametrů A–E ve vlastních sloupcích
        public static byte[] ToCsv(IReadOnlyCollection<OpenPreAuditDto> rows)
        {
            var csv = new StringBuilder();
            string[] header =
            [
                "id_objektu", "kod", "nazev", "kategorie", "okres_kod", "orp_kod", "lat", "lon", "skore", "stav", "hlavni_pricina",
                "a_obalka_body", "b_klima_body", "c_ekonomika_body", "d_hustota_osob_body", "e_okoli_body",
                "merna_ztrata_w_m2k", "tepelna_ztrata_w_k", "denostupne", "potreba_tepla_kwh_rok", "cena_tepla_ze_smlouvy",
                "uspora_regulaci_pct", "iqrf_brany", "iqrf_cidla", "iqrf_cena_sady_czk", "vypocteno",
            ];
            csv.AppendJoin(',', header).Append("\r\n");

            foreach (var r in rows)
            {
                var points = Parameters.Select(k => r.Parameters.FirstOrDefault(p => p.Key == k)?.Score.ToString(CultureInfo.InvariantCulture));

                string?[] row =
                [
                    r.FacilityId.ToString(), r.Code, r.Name, r.Category.ToString(), r.DistrictCode, r.OrpCode, Number(r.Latitude), Number(r.Longitude),
                    r.Score.ToString(CultureInfo.InvariantCulture), r.Status.ToString(), r.DominantDriver,
                    .. points,
                    Number(r.SpecificHeatLoss), Number(r.HeatLossWattsPerKelvin), Number(r.DegreeDays), Number(r.AnnualHeatDemandKwh),
                    r.HeatPriceFromContract ? "true" : "false", Number(r.RegulationSavingPercent),
                    r.IqrfGateways.ToString(CultureInfo.InvariantCulture), r.IqrfSensors.ToString(CultureInfo.InvariantCulture),
                    r.IqrfHardwareCostCzk.ToString("0", CultureInfo.InvariantCulture), r.ComputedAt.ToString("O", CultureInfo.InvariantCulture),
                ];

                csv.AppendJoin(',', row.Select(Escape)).Append("\r\n");
            }

            return [.. Encoding.UTF8.GetPreamble(), .. Encoding.UTF8.GetBytes(csv.ToString())];
        }

        private static string? Number(double value) => value.ToString("0.######", CultureInfo.InvariantCulture);

        private static string Escape(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return value.IndexOfAny([',', '"', '\r', '\n']) >= 0 ? $"\"{value.Replace("\"", "\"\"")}\"" : value;
        }
    }
}
