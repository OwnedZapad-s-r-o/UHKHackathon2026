using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAudit.Api.Extentions;
using SmartAudit.Core.Common;
using SmartAudit.Core.Data;
using SmartAudit.Core.Domain;
using SmartAudit.Core.Domain.Enums;
using SmartAudit.Core.Domain.Helpers;
using SmartAudit.Core.Domain.Settings;

namespace SmartAudit.Api.Features.Facilities.GetContracts
{
    public record GetContractsCommand(Guid FacilityId, ViewerContext Viewer) : IRequest<Response<GetContractsDto>>;

    // SyncedAt = null → smlouvy organizace se z registru ještě nestáhly (běží na pozadí po startu)
    public record GetContractsDto(
        string? OrganizationName,
        string? Ico,
        DateTimeOffset? SyncedAt,
        HeatPriceDto HeatPrice,
        IReadOnlyCollection<ContractDto> Contracts);

    // Cena tepla, se kterou počítá audit — ze smlouvy, nebo paušál z konfigurace
    public record HeatPriceDto(decimal CzkPerKwh, decimal DefaultCzkPerKwh, bool FromContract, Guid? ContractId, decimal? AnnualLossCzk);

    public record ContractDto(
        Guid Id,
        string ContractId,
        string? ContractNumber,
        string Title,
        ContractCategory Category,
        string PartnerName,
        string? PartnerIco,
        decimal? AmountWithoutVat,
        decimal? AmountWithVat,
        DateOnly? DateSigned,
        DateOnly PublishedOn,
        string RegistryUrl,
        EnergyCommodity? Commodity,
        double? UnitPriceCzkPerMwh,
        string? PriceSource,
        IReadOnlyCollection<ContractAttachmentDto> Attachments);

    public record ContractAttachmentDto(string Id, string Name);

    // Smlouvy organizace, která objekt spravuje, z Registru smluv — energie, servis kotelen a MaR, stavební úpravy
    public class GetContractsHandler : IRequestHandler<GetContractsCommand, Response<GetContractsDto>>
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly SmartAuditDbContext _dbContext;
        private readonly AuditSettings _settings;

        public GetContractsHandler(SmartAuditDbContext dbContext, AuditSettings settings)
        {
            _dbContext = dbContext;
            _settings = settings;
        }

        public async Task<Response<GetContractsDto>> Handle(GetContractsCommand request, CancellationToken ct)
        {
            var facility = await _dbContext.Facilities
                .AsNoTracking()
                .Where(f => f.Id == request.FacilityId)
                .Select(f => new
                {
                    f.OrganizationId,
                    OrganizationName = f.Organization == null ? null : f.Organization.Name,
                    Ico = f.Organization == null ? null : f.Organization.Ico,
                    SyncedAt = f.Organization == null ? null : f.Organization.ContractsSyncedAt,
                    AnnualLossCzk = f.Assessment == null ? (decimal?)null : f.Assessment.AnnualLossCzk,
                })
                .FirstOrDefaultAsync(ct);

            if (facility == null)
                return Response.NotFound<GetContractsDto>("Objekt s tímto id neexistuje");

            if (request.Viewer.Role == AppRoles.Manager && facility.OrganizationId != request.Viewer.OrganizationId)
                return Response.Forbidden<GetContractsDto>("K tomuto nemáte oprávnění");

            var rows = facility.OrganizationId == null ? [] : await _dbContext.FacilityContracts
                .AsNoTracking()
                .Where(c => c.OrganizationId == facility.OrganizationId)
                .OrderBy(c => c.Category)
                .ThenByDescending(c => c.DateSigned ?? c.PublishedOn)
                .ToListAsync(ct);

            var heatPrice = EnergyPriceExtractor.SelectHeatPrice(
                rows.Where(c => c.Category == ContractCategory.EnergySupply).Select(c => new ContractPrice(c.Id, c.Commodity, c.UnitPriceCzkPerMwh, c.DateSigned)),
                _settings.HeatPriceCzkPerKwh);

            var contracts = rows
                .Select(c => new ContractDto(
                    c.Id,
                    c.ContractId,
                    c.ContractNumber,
                    c.Title,
                    c.Category,
                    c.PartnerName,
                    c.PartnerIco,
                    c.AmountWithoutVat,
                    c.AmountWithVat,
                    c.DateSigned,
                    c.PublishedOn,
                    ContractRegistryParser.DetailUrl(c.VersionId),
                    c.Commodity,
                    c.UnitPriceCzkPerMwh,
                    c.PriceSource,
                    (JsonSerializer.Deserialize<List<RegistryAttachment>>(c.AttachmentsJson, JsonOptions) ?? []).Select(a => new ContractAttachmentDto(a.Id, a.Name)).ToList()))
                .ToList();

            var dto = new GetContractsDto(
                facility.OrganizationName,
                facility.Ico,
                facility.SyncedAt,
                new HeatPriceDto(heatPrice.CzkPerKwh, _settings.HeatPriceCzkPerKwh, heatPrice.FromContract, heatPrice.ContractId, facility.AnnualLossCzk),
                contracts);

            return Response.Success(dto);
        }
    }
}
