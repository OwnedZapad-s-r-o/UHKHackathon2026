using MediatR;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using SmartAudit.Api.Extentions;
using SmartAudit.Api.Services;
using SmartAudit.Api.Services.Audit;
using SmartAudit.Core.Common;
using SmartAudit.Core.Data;
using SmartAudit.Core.Data.Tables;
using SmartAudit.Core.Domain;
using SmartAudit.Core.Domain.Enums;
using SmartAudit.Core.Domain.Helpers;

namespace SmartAudit.Api.Features.Facilities.CreateFacility
{
    // Tělo požadavku — organizaci doplní controller z tokenu. Floors a ConstructionYear jsou opravy správce
    // (katastr podlaží nevede a rok výstavby často chybí); bez nich platí údaje z ČÚZK a odhad.
    public record CreateFacilityRequest(
        string Name,
        FacilityCategory Category,
        string? Kind,
        double Latitude,
        double Longitude,
        string? Street,
        string? HouseNumber,
        string? Psc,
        int? Floors,
        int? ConstructionYear);

    public record CreateFacilityCommand(
        ViewerContext Viewer,
        string Name,
        FacilityCategory Category,
        string? Kind,
        double Latitude,
        double Longitude,
        string? Street,
        string? HouseNumber,
        string? Psc,
        int? Floors,
        int? ConstructionYear) : IRequest<Response<CreateFacilityDto>>;

    public record CreateFacilityDto(Guid Id, string Code);

    // Správce přidá budovu své organizace do mapy. Obec, ORP, okres, památková ochrana a karta budovy se dopočítají
    // stejně jako u importu z otevřených dat; simulovaná čidla a první vyhodnocení auditu doplní smyčka simulace hned potom.
    public class CreateFacilityHandler : IRequestHandler<CreateFacilityCommand, Response<CreateFacilityDto>>
    {
        private const int MaxFloors = 30;

        private static readonly GeometryFactory Factory = new(new PrecisionModel(), 4326);

        private readonly SmartAuditDbContext _dbContext;
        private readonly IPlaceLookupService _placeLookup;
        private readonly IAuditTrigger _auditTrigger;

        public CreateFacilityHandler(SmartAuditDbContext dbContext, IPlaceLookupService placeLookup, IAuditTrigger auditTrigger)
        {
            _dbContext = dbContext;
            _placeLookup = placeLookup;
            _auditTrigger = auditTrigger;
        }

        public async Task<Response<CreateFacilityDto>> Handle(CreateFacilityCommand request, CancellationToken ct)
        {
            if (request.Viewer.Role != AppRoles.Manager || request.Viewer.OrganizationId == null)
                return Response.Forbidden<CreateFacilityDto>("Budovu může přidat jen správce budovy své organizace");

            var name = request.Name?.Trim() ?? string.Empty;
            if (name.Length < 3 || name.Length > 300)
                return Response.ValidationFailed<CreateFacilityDto>("Název budovy musí mít 3 až 300 znaků");

            if (!Enum.IsDefined(request.Category))
                return Response.ValidationFailed<CreateFacilityDto>("Vyberte, k čemu budova slouží");

            var kind = string.IsNullOrWhiteSpace(request.Kind) ? null : request.Kind.Trim();
            if (kind is { Length: > 100 })
                return Response.ValidationFailed<CreateFacilityDto>("Typ budovy může mít nejvýš 100 znaků");

            if (request.Floors is < 1 or > MaxFloors)
                return Response.ValidationFailed<CreateFacilityDto>($"Počet podlaží musí být 1 až {MaxFloors}");

            if (request.ConstructionYear is { } year && (year < 1200 || year > DateTime.UtcNow.Year))
                return Response.ValidationFailed<CreateFacilityDto>("Rok výstavby není platný");

            if (request.Latitude is not (>= -90 and <= 90) || request.Longitude is not (>= -180 and <= 180))
                return Response.ValidationFailed<CreateFacilityDto>("Vyberte místo budovy v mapě");

            if (Longer(request.Street, 200) || Longer(request.HouseNumber, 20) || Longer(request.Psc, 10))
                return Response.ValidationFailed<CreateFacilityDto>("Adresa je příliš dlouhá");

            var organization = await _dbContext.Organizations
                .AsNoTracking()
                .Select(o => new { o.Id, o.Ico, o.Web })
                .FirstOrDefaultAsync(o => o.Id == request.Viewer.OrganizationId, ct);

            if (organization == null)
                return Response.NotFound<CreateFacilityDto>("Organizace s tímto id neexistuje");

            var location = Factory.CreatePoint(new Coordinate(request.Longitude, request.Latitude));
            var place = await _placeLookup.LookupAsync(location, ct);

            if (place.Territory is not { } territory)
                return Response.ValidationFailed<CreateFacilityDto>("Místo leží mimo Královéhradecký kraj — aplikace spravuje jen budovy v kraji");

            // Stejná budova z katastru už ve vlastních budovách je — druhý záznam by zdvojil čidla i audit
            var duplicate = place.Neighbors.FirstOrDefault(n => n.SameBuilding && n.OrganizationId == organization.Id);
            if (duplicate != null)
                return Response.ValidationFailed<CreateFacilityDto>($"Tahle budova už v mapě je jako „{duplicate.Name}“ ({duplicate.Code})");

            var sourceKey = $"{Facility.ManagerSourcePrefix}{Guid.CreateVersion7():N}";

            var facility = new Facility(sourceKey, name, request.Category, location);
            facility.SetSimulationProfile(FacilityDefaults.SimulationProfileFor(sourceKey, request.Category));
            facility.SetIdentifiers(kind == null ? [] : [kind], [], null, organization.Ico, organization.Web);
            facility.SetAddress(
                Clean(request.Street),
                Clean(request.HouseNumber),
                Clean(request.Psc)?.Replace(" ", string.Empty),
                territory.MunicipalityCode,
                territory.MunicipalityName,
                territory.OrpCode,
                territory.OrpName,
                territory.DistrictCode,
                territory.DistrictName);
            facility.AssignOrganization(organization.Id);
            facility.SetHeritage(place.HeritageStatus, place.HeritageName);
            facility.AssignCode(await NextCodeAsync(request.Category, territory.OrpCode, ct));

            _dbContext.Facilities.Add(facility);

            var cadastre = place.Building;
            var constructionYear = request.ConstructionYear ?? cadastre?.ConstructionYear;

            var building = new FacilityBuilding(facility.Id);
            building.SetCadastre(cadastre?.Footprint, cadastre?.BuiltUpArea, constructionYear, cadastre?.CurrentUse, cadastre?.Condition, cadastre?.ParcelRef, cadastre?.RuianCode);
            building.SetEstimates(BuildingEstimator.CategoryFromYear(constructionYear), request.Floors ?? BuildingEstimator.EstimateFloors(cadastre?.BuiltUpArea, request.Category));
            _dbContext.FacilityBuildings.Add(building);

            await _dbContext.SaveChangesAsync(ct);

            _auditTrigger.RequestProvisioning(facility.Id);

            return Response.Success(new CreateFacilityDto(facility.Id, facility.Code));
        }

        // Kód jako u importu (kategorie-ORP-pořadí), s „S“ před pořadím — import čísluje svoje objekty znovu
        // a s budovami správců se tak nikdy nepotká
        private async Task<string> NextCodeAsync(FacilityCategory category, string orpCode, CancellationToken ct)
        {
            var prefix = $"{FacilityDefaults.CodePrefix(category)}-{orpCode}-S";

            var used = await _dbContext.Facilities
                .Where(f => f.Code.StartsWith(prefix))
                .Select(f => f.Code)
                .ToListAsync(ct);

            var order = 1;
            while (used.Contains($"{prefix}{order:00}"))
                order++;

            return $"{prefix}{order:00}";
        }

        private static string? Clean(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static bool Longer(string? value, int max)
        {
            return value != null && value.Trim().Length > max;
        }
    }
}
