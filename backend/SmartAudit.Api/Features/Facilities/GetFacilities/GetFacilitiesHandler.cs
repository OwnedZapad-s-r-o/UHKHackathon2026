using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAudit.Api.Extentions;
using SmartAudit.Core.Common;
using SmartAudit.Core.Data;
using SmartAudit.Core.Domain;
using SmartAudit.Core.Domain.Enums;

namespace SmartAudit.Api.Features.Facilities.GetFacilities
{
    public record GetFacilitiesCommand(ViewerContext Viewer, string? Query = null, FacilityCategory? Category = null, string? District = null, string? Orp = null, AuditStatus? Status = null, bool OnlyIssues = false)
        : IRequest<Response<IReadOnlyCollection<GetFacilityDto>>>;

    public record GetFacilityDto(
        Guid Id,
        string Code,
        string Name,
        FacilityCategory Category,
        string Kind,
        string MunicipalityName,
        string OrpCode,
        string OrpName,
        string DistrictCode,
        string DistrictName,
        double Latitude,
        double Longitude,
        AuditStatus Status,
        string Headline,
        double? Temperature,
        double? Co2,
        double? Power,
        decimal? AnnualLossCzk,
        HeritageStatus HeritageStatus);

    // Mapa a tabulka Portfolio — jen data z cache vyhodnocení, bez sahání na miliony měření
    public class GetFacilitiesHandler : IRequestHandler<GetFacilitiesCommand, Response<IReadOnlyCollection<GetFacilityDto>>>
    {
        private readonly SmartAuditDbContext _dbContext;

        public GetFacilitiesHandler(SmartAuditDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Response<IReadOnlyCollection<GetFacilityDto>>> Handle(GetFacilitiesCommand request, CancellationToken ct)
        {
            var query = _dbContext.Facilities.AsNoTracking();

            // Správce budovy vidí jen objekty své organizace
            if (request.Viewer.Role == AppRoles.Manager)
                query = query.Where(f => f.OrganizationId == request.Viewer.OrganizationId);

            if (!string.IsNullOrWhiteSpace(request.Query))
            {
                var pattern = $"%{request.Query.Trim()}%";
                query = query.Where(f => EF.Functions.ILike(f.Name, pattern)
                    || EF.Functions.ILike(f.Code, pattern)
                    || EF.Functions.ILike(f.MunicipalityName, pattern)
                    || EF.Functions.ILike(f.OrpName, pattern)
                    || (f.Street != null && EF.Functions.ILike(f.Street, pattern)));
            }

            if (request.Category != null)
                query = query.Where(f => f.Category == request.Category);

            if (!string.IsNullOrWhiteSpace(request.District))
                query = query.Where(f => f.DistrictCode == request.District);

            if (!string.IsNullOrWhiteSpace(request.Orp))
                query = query.Where(f => f.OrpCode == request.Orp);

            if (request.Status != null)
                query = query.Where(f => f.Assessment != null && f.Assessment.Status == request.Status);

            if (request.OnlyIssues)
                query = query.Where(f => f.Assessment != null && f.Assessment.Status != AuditStatus.Nominal);

            var canSeeMoney = request.Viewer.CanSeeMoney;

            var facilities = await query
                .OrderByDescending(f => f.Assessment != null ? f.Assessment.AnnualLossCzk : 0)
                .ThenBy(f => f.Code)
                .Select(f => new GetFacilityDto(
                    f.Id,
                    f.Code,
                    f.Name,
                    f.Category,
                    f.Kinds.Length > 0 ? f.Kinds[0] : string.Empty,
                    f.MunicipalityName,
                    f.OrpCode,
                    f.OrpName,
                    f.DistrictCode,
                    f.DistrictName,
                    f.Location.Y,
                    f.Location.X,
                    f.Assessment != null ? f.Assessment.Status : AuditStatus.Nominal,
                    f.Assessment != null ? f.Assessment.Headline : "Čeká na první vyhodnocení",
                    f.Assessment != null ? f.Assessment.LatestTemperature : null,
                    f.Assessment != null ? f.Assessment.LatestCo2 : null,
                    f.Assessment != null ? f.Assessment.LatestPower : null,
                    canSeeMoney && f.Assessment != null ? f.Assessment.AnnualLossCzk : null,
                    f.HeritageStatus))
                .ToListAsync(ct);

            return Response.Success<IReadOnlyCollection<GetFacilityDto>>(facilities);
        }
    }
}
