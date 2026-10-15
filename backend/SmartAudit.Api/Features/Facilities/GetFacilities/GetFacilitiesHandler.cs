using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAudit.Api.Extentions;
using SmartAudit.Core.Common;
using SmartAudit.Core.Data;
using SmartAudit.Core.Domain;
using SmartAudit.Core.Domain.Enums;
using SmartAudit.Core.Domain.Helpers;

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
        HeritageStatus HeritageStatus,
        // Mění se s doplněnými přilehlými budovami — mapa ji přidá k adrese obrysu, který se jinak cachuje 2 dny
        string FootprintVersion,
        // Pre-audit z otevřených dat (0–100) a návratnost sady IQRF v měsících (jen pro ty, kdo vidí peníze)
        int? PreAuditScore = null,
        string? PreAuditDriver = null,
        decimal? IqrfHardwareCostCzk = null,
        double? IqrfPaybackMonths = null,
        // Teplotu měří skutečné čidlo IQRF (jinak fyzikální model z otevřených dat)
        bool LiveMeasurement = false);

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
                    f.HeritageStatus,
                    "0",
                    f.PreAudit != null ? f.PreAudit.Score : null,
                    f.PreAudit != null ? f.PreAudit.DominantDriver : null,
                    f.PreAudit != null ? f.PreAudit.IqrfHardwareCostCzk : null,
                    null,
                    false))
                .ToListAsync(ct);

            // Návratnost IQRF = cena sady / (větší z ztráty a potenciálu regulace) — z cache pre-auditu a vyhodnocení
            var roiInputs = await _dbContext.FacilityPreAudits
                .AsNoTracking()
                .Select(p => new { p.FacilityId, p.IqrfGateways, p.IqrfSensors, p.IqrfHardwareCostCzk, p.RegulationPotentialCzk, Loss = p.Facility.Assessment != null ? p.Facility.Assessment.AnnualLossCzk : 0 })
                .ToDictionaryAsync(p => p.FacilityId, ct);

            var liveFacilities = (await _dbContext.Sensors
                .AsNoTracking()
                .Where(s => s.Source == SensorSource.Physical && s.IsActive && s.Metric == SensorMetric.Temperature)
                .Select(s => s.FacilityId)
                .Distinct()
                .ToListAsync(ct))
                .ToHashSet();

            facilities = facilities
                .Select(f =>
                {
                    double? payback = null;
                    if (canSeeMoney && roiInputs.TryGetValue(f.Id, out var r))
                        payback = IqrfRoiCalculator.Calculate(new IqrfKit(r.IqrfGateways, r.IqrfSensors, r.IqrfHardwareCostCzk), r.Loss, r.RegulationPotentialCzk).PaybackMonths;

                    return f with { IqrfPaybackMonths = payback, LiveMeasurement = liveFacilities.Contains(f.Id) };
                })
                .ToList();

            // Doplněných budov a oprav je málo — verze obrysu jedním dotazem pro všechny objekty:
            // počet doplněných budov + poslední změna budovy nebo oprava podlaží (mění výšku ve 3D)
            var parts = await _dbContext.FacilityBuildingParts
                .AsNoTracking()
                .GroupBy(p => p.FacilityId)
                .Select(g => new { FacilityId = g.Key, Count = g.Count(), LastChange = g.Max(p => p.UpdatedAt) })
                .ToDictionaryAsync(g => g.FacilityId, ct);

            var edits = await _dbContext.FacilityBuildings
                .AsNoTracking()
                .Where(b => b.EditedAt != null)
                .Select(b => new { b.FacilityId, EditedAt = b.EditedAt!.Value })
                .ToDictionaryAsync(b => b.FacilityId, b => b.EditedAt, ct);

            facilities = facilities
                .Select(f =>
                {
                    var partVersion = parts.TryGetValue(f.Id, out var p) ? $"{p.Count}-{p.LastChange.ToUnixTimeMilliseconds()}" : "0";
                    var editVersion = edits.TryGetValue(f.Id, out var edited) ? edited.ToUnixTimeMilliseconds().ToString() : "0";
                    return partVersion == "0" && editVersion == "0" ? f : f with { FootprintVersion = $"{partVersion}-{editVersion}" };
                })
                .ToList();

            return Response.Success<IReadOnlyCollection<GetFacilityDto>>(facilities);
        }
    }
}
