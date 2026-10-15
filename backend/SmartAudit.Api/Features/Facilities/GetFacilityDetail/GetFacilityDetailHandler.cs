using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using SmartAudit.Api.Extentions;
using SmartAudit.Api.Services.Sensors;
using SmartAudit.Core.Common;
using SmartAudit.Core.Data;
using SmartAudit.Core.Data.Tables;
using SmartAudit.Core.Domain;
using SmartAudit.Core.Domain.Enums;
using SmartAudit.Core.Domain.Helpers;

namespace SmartAudit.Api.Features.Facilities.GetFacilityDetail
{
    public record GetFacilityDetailCommand(Guid FacilityId, ViewerContext Viewer) : IRequest<Response<GetFacilityDetailDto>>;

    public record GetFacilityDetailDto(
        Guid Id,
        string Code,
        string Name,
        FacilityCategory Category,
        string[] Kinds,
        AddressDto Address,
        double Latitude,
        double Longitude,
        string? Web,
        OrganizationDto? Organization,
        string Department,
        BuildingDto? Building,
        IReadOnlyCollection<OccupancyDto> Occupancy,
        TerritoryDto Territory,
        HeritageDto Heritage,
        MunicipalityContactDto? MunicipalityContact,
        AssessmentDto? Assessment,
        int ZoneCount,
        int SensorCount,
        // Budovu přidal do mapy správce (není v otevřených datech) — jen takovou může také odebrat
        bool AddedByManager,
        IReadOnlyCollection<BuildingPartDto> AddedBuildingParts);

    // Přilehlá budova doplněná správcem (katastr ji k objektu nevede) — její plocha vstupuje do výpočtu ztráty
    public record BuildingPartDto(Guid Id, double AreaSquareMeters, string Source, DateTimeOffset AddedAt);

    public record AddressDto(string? Street, string? HouseNumber, string? Psc, string Municipality, string MunicipalityCode, string Orp, string OrpCode, string District, string DistrictCode, string Label);

    public record OrganizationDto(Guid Id, string Ico, string Name, string? LegalForm, string? Web, string? PhotoUrl, string? RelationType);

    // BuiltUpArea, ConstructionYear, EnergyCategory, EstimatedFloors = údaje z ČÚZK a odhadu; *Override = opravy od správce
    // (mají přednost ve výpočtu ztráty), EditedEnergyCategory = kategorie podle opraveného roku výstavby
    public record BuildingDto(
        bool Found,
        Geometry? Footprint,
        double? BuiltUpArea,
        int? ConstructionYear,
        string EnergyCategory,
        int EstimatedFloors,
        string? CurrentUse,
        string? RuianCode,
        string? CadastreUrl,
        string? ParcelRef,
        double? AreaOverride,
        int? FloorsOverride,
        int? YearOverride,
        string? EditedEnergyCategory,
        DateTimeOffset? EditedAt);

    public record OccupancyDto(string SchoolYear, int? Students, int? Graduates, int? Admitted, int? BedCapacity, int? Residents, double? OccupancyPercent, int? Applicants, int? OfferedPlaces, double? DemandRatio);

    public record TerritoryDto(bool IsThreatenedArea, string? ThreatLevel, GrantsDto? OrpGrants);

    public record GrantsDto(string OrpName, int? Count, decimal? Total, int? IndividualCount, decimal? IndividualTotal, int? ProgramCount, decimal? ProgramTotal);

    public record HeritageDto(HeritageStatus Status, string? Name, string? Constraint);

    public record MunicipalityContactDto(string Municipality, string? DataBox, string? Email, string? Phone, string? Web, string? Address);

    public record AssessmentDto(
        AuditStatus Status,
        string Headline,
        string Summary,
        IReadOnlyCollection<FindingDto> Findings,
        double? Temperature,
        double? Co2,
        double? Power,
        double Co2HoursOverLimit,
        double? FreeDayMeanTemperature,
        double? NightMeanTemperature,
        double WeeklyEnergyKwh,
        decimal? AnnualLossCzk,
        double? SavingPercent,
        DateTimeOffset EvaluatedAt,
        SensorCorrectionDto? SensorCorrection);

    // Jak skutečná čidla opravila odhad ztráty: ztráta před prvním čidlem teploty (odhad ze simulace) a kolik místností měří.
    // VirtualHistory = aspoň jedno čidlo je virtuální a dodalo týden historie; skutečné čidlo ztrátu upřesňuje postupně 7 dní.
    public record SensorCorrectionDto(decimal LossBeforeCzk, int MeasuredZones, int ZoneCount, bool VirtualHistory, DateTimeOffset Since);

    public record FindingDto(string Title, string Detail, string Tag, string Severity);

    // Inspekční panel — spojuje objekt s daty z dalších sad (organizace, ČÚZK, obsazenost, HSOO, dotace, památky, kontakt obce)
    public class GetFacilityDetailHandler : IRequestHandler<GetFacilityDetailCommand, Response<GetFacilityDetailDto>>
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly SmartAuditDbContext _dbContext;

        public GetFacilityDetailHandler(SmartAuditDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Response<GetFacilityDetailDto>> Handle(GetFacilityDetailCommand request, CancellationToken ct)
        {
            var facility = await _dbContext.Facilities
                .AsNoTracking()
                .Include(f => f.Organization)
                .Include(f => f.Building)
                .Include(f => f.Assessment)
                .Include(f => f.Metrics)
                .AsSplitQuery()
                .FirstOrDefaultAsync(f => f.Id == request.FacilityId, ct);

            if (facility == null)
                return Response.NotFound<GetFacilityDetailDto>("Objekt s tímto id neexistuje");

            if (request.Viewer.Role == AppRoles.Manager && facility.OrganizationId != request.Viewer.OrganizationId)
                return Response.Forbidden<GetFacilityDetailDto>("K tomuto nemáte oprávnění");

            var municipality = await _dbContext.Regions
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Type == RegionType.Obec && r.Code == facility.MunicipalityCode, ct);

            var orp = await _dbContext.Regions
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Type == RegionType.Orp && r.Code == facility.OrpCode, ct);

            var contact = await _dbContext.MunicipalityContacts
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.MunicipalityCode == facility.MunicipalityCode, ct);

            var zoneCount = await _dbContext.Zones.CountAsync(z => z.FacilityId == facility.Id, ct);
            var sensorCount = await _dbContext.Sensors.CountAsync(s => s.FacilityId == facility.Id, ct);

            var buildingParts = await _dbContext.FacilityBuildingParts
                .AsNoTracking()
                .Where(p => p.FacilityId == facility.Id)
                .OrderBy(p => p.AddedAt)
                .Select(p => new BuildingPartDto(p.Id, p.AreaSquareMeters, p.Source, p.AddedAt))
                .ToListAsync(ct);

            var canSeeMoney = request.Viewer.CanSeeMoney;

            // Oprava ztráty čidly — jen pro ty, kdo vidí peníze
            SensorCorrectionDto? correction = null;
            if (canSeeMoney && facility.Assessment is { LossBeforeSensorsCzk: { } lossBefore, SensorsSince: { } since })
            {
                var measured = (await MeasuredZones.QueryAsync(_dbContext, [facility.Id], ct)).GetValueOrDefault(facility.Id);
                correction = new SensorCorrectionDto(lossBefore, measured?.Count ?? 0, zoneCount, measured?.Virtual ?? false, since);
            }

            var houseNumber = facility.HouseNumber;
            var street = facility.Street ?? facility.MunicipalityName;

            var address = new AddressDto(
                facility.Street,
                houseNumber,
                facility.Psc,
                facility.MunicipalityName,
                facility.MunicipalityCode,
                facility.OrpName,
                facility.OrpCode,
                facility.DistrictName,
                facility.DistrictCode,
                $"{street} {houseNumber}, {facility.Psc} {facility.MunicipalityName}".Replace("  ", " ").Trim());

            var organization = facility.Organization == null ? null : new OrganizationDto(
                facility.Organization.Id,
                facility.Organization.Ico,
                facility.Organization.Name,
                facility.Organization.LegalForm,
                facility.Organization.Web,
                facility.Organization.PhotoUrl,
                facility.Organization.RelationType);

            var building = facility.Building == null ? null : new BuildingDto(
                facility.Building.Found,
                facility.Building.Footprint,
                facility.Building.BuiltUpArea,
                facility.Building.ConstructionYear,
                BuildingEstimator.CategoryLabel(facility.Building.EnergyCategory),
                facility.Building.EstimatedFloors,
                facility.Building.CurrentUse,
                facility.Building.RuianCode,
                facility.Building.RuianCode == null ? null : $"https://vdp.cuzk.gov.cz/vdp/ruian/stavebniobjekty/{facility.Building.RuianCode}",
                facility.Building.ParcelRef,
                facility.Building.AreaOverride,
                facility.Building.FloorsOverride,
                facility.Building.YearOverride,
                facility.Building.YearOverride == null ? null : BuildingEstimator.CategoryLabel(facility.Building.EffectiveEnergyCategory),
                facility.Building.EditedAt);

            var occupancy = facility.Metrics
                .OrderByDescending(m => m.SchoolYear)
                .Select(m => new OccupancyDto(
                    m.SchoolYear,
                    m.Students,
                    m.Graduates,
                    m.Admitted,
                    m.BedCapacity,
                    m.Residents,
                    m.BedCapacity is > 0 ? Math.Round(100.0 * (m.Residents ?? 0) / m.BedCapacity.Value, 1) : null,
                    m.Applicants,
                    m.OfferedPlaces,
                    m.OfferedPlaces is > 0 ? Math.Round((double)(m.Applicants ?? 0) / m.OfferedPlaces.Value, 2) : null))
                .ToList();

            var grants = orp == null ? null : new GrantsDto(orp.Name, orp.GrantCount, orp.GrantTotal, orp.IndividualGrantCount, orp.IndividualGrantTotal, orp.ProgramGrantCount, orp.ProgramGrantTotal);
            var territory = new TerritoryDto(municipality?.IsThreatened ?? false, municipality?.ThreatLevel, grants);

            var heritage = new HeritageDto(facility.HeritageStatus, facility.HeritageName, HeritageConstraint(facility.HeritageStatus));

            var municipalityContact = contact == null ? null : new MunicipalityContactDto(contact.MunicipalityName, contact.DataBox, contact.Email, contact.Phone, contact.Web, contact.Address);

            var assessment = facility.Assessment == null ? null : new AssessmentDto(
                facility.Assessment.Status,
                facility.Assessment.Headline,
                facility.Assessment.Summary,
                JsonSerializer.Deserialize<List<FindingDto>>(facility.Assessment.FindingsJson, JsonOptions) ?? [],
                facility.Assessment.LatestTemperature,
                facility.Assessment.LatestCo2,
                facility.Assessment.LatestPower,
                facility.Assessment.Co2HoursOverLimit,
                facility.Assessment.FreeDayMeanTemperature,
                facility.Assessment.NightMeanTemperature,
                facility.Assessment.WeeklyEnergyKwh,
                canSeeMoney ? facility.Assessment.AnnualLossCzk : null,
                canSeeMoney ? facility.Assessment.SavingPercent : null,
                facility.Assessment.EvaluatedAt,
                correction);

            var dto = new GetFacilityDetailDto(
                facility.Id,
                facility.Code,
                facility.Name,
                facility.Category,
                facility.Kinds,
                address,
                facility.Location.Y,
                facility.Location.X,
                facility.Web ?? facility.Organization?.Web,
                organization,
                DepartmentLabel(facility.Organization?.Department, facility.Category),
                building,
                occupancy,
                territory,
                heritage,
                municipalityContact,
                assessment,
                zoneCount,
                sensorCount,
                facility.SourceKey.StartsWith(Facility.ManagerSourcePrefix),
                buildingParts);

            return Response.Success(dto);
        }

        // Sada Organizace KHK uvádí odbor jen u přímo zřizovaných organizací — u ostatních podle oblasti
        private static string DepartmentLabel(string? department, FacilityCategory category)
        {
            var name = department ?? category switch
            {
                FacilityCategory.School => "školství",
                FacilityCategory.Social => "sociálních věcí",
                FacilityCategory.Health => "zdravotnictví",
                _ => "kultury a památkové péče",
            };

            return $"Odbor {name} KÚ KHK";
        }

        private static string? HeritageConstraint(HeritageStatus status)
        {
            return status switch
            {
                HeritageStatus.NationalMonument => "Národní kulturní památka — zásahy do obálky budovy (zateplení, okna) jen se souhlasem památkové péče. Přednost mají opatření v regulaci a technice.",
                HeritageStatus.Reserve => "Památková rezervace — změny fasády a oken vyžadují závazné stanovisko orgánu památkové péče.",
                HeritageStatus.Zone => "Památková zóna — vnější zateplení a výměna oken podléhají stanovisku památkářů; bez omezení jsou opatření v regulaci vytápění.",
                _ => null,
            };
        }
    }
}
