using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAudit.Core.Data;
using SmartAudit.Core.Domain;
using SmartAudit.Core.Domain.Enums;

namespace SmartAudit.Api.Features.OpenData.GetOpenFacilities
{
    public record GetOpenFacilitiesCommand(string? Query = null, FacilityCategory? Category = null, string? District = null, AuditStatus? Status = null)
        : IRequest<Response<IReadOnlyCollection<OpenFacilityDto>>>;

    // Veřejný záznam objektu — bez částek v Kč a bez vazby na správce; stejná data vidí nepřihlášený návštěvník v mapě
    public record OpenFacilityDto(
        Guid Id,
        string Code,
        string Name,
        FacilityCategory Category,
        string Kind,
        string? Ico,
        string? Street,
        string? HouseNumber,
        string? Psc,
        string MunicipalityCode,
        string MunicipalityName,
        string OrpCode,
        string OrpName,
        string DistrictCode,
        string DistrictName,
        double Latitude,
        double Longitude,
        HeritageStatus HeritageStatus,
        bool HasFootprint,
        AuditStatus Status,
        string Headline,
        double? Temperature,
        double? Co2,
        double? Power,
        DateTimeOffset? EvaluatedAt);

    // Seznam objektů pro otevřené API — JSON, GeoJSON i CSV staví nad stejným výsledkem
    public class GetOpenFacilitiesHandler : IRequestHandler<GetOpenFacilitiesCommand, Response<IReadOnlyCollection<OpenFacilityDto>>>
    {
        private readonly SmartAuditDbContext _dbContext;

        public GetOpenFacilitiesHandler(SmartAuditDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Response<IReadOnlyCollection<OpenFacilityDto>>> Handle(GetOpenFacilitiesCommand request, CancellationToken ct)
        {
            var query = _dbContext.Facilities.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.Query))
            {
                var pattern = $"%{request.Query.Trim()}%";
                query = query.Where(f => EF.Functions.ILike(f.Name, pattern)
                    || EF.Functions.ILike(f.Code, pattern)
                    || EF.Functions.ILike(f.MunicipalityName, pattern));
            }

            if (request.Category != null)
                query = query.Where(f => f.Category == request.Category);

            if (!string.IsNullOrWhiteSpace(request.District))
                query = query.Where(f => f.DistrictCode == request.District);

            if (request.Status != null)
                query = query.Where(f => f.Assessment != null && f.Assessment.Status == request.Status);

            var facilities = await query
                .OrderBy(f => f.Code)
                .Select(f => new OpenFacilityDto(
                    f.Id,
                    f.Code,
                    f.Name,
                    f.Category,
                    f.Kinds.Length > 0 ? f.Kinds[0] : string.Empty,
                    f.Ico,
                    f.Street,
                    f.HouseNumber,
                    f.Psc,
                    f.MunicipalityCode,
                    f.MunicipalityName,
                    f.OrpCode,
                    f.OrpName,
                    f.DistrictCode,
                    f.DistrictName,
                    f.Location.Y,
                    f.Location.X,
                    f.HeritageStatus,
                    f.Building != null && f.Building.Footprint != null,
                    f.Assessment != null ? f.Assessment.Status : AuditStatus.Nominal,
                    f.Assessment != null ? f.Assessment.Headline : "Čeká na první vyhodnocení",
                    f.Assessment != null ? f.Assessment.LatestTemperature : null,
                    f.Assessment != null ? f.Assessment.LatestCo2 : null,
                    f.Assessment != null ? f.Assessment.LatestPower : null,
                    f.Assessment != null ? f.Assessment.EvaluatedAt : null))
                .ToListAsync(ct);

            return Response.Success<IReadOnlyCollection<OpenFacilityDto>>(facilities);
        }
    }
}
