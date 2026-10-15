using MediatR;
using NetTopologySuite.Geometries;
using SmartAudit.Api.Extentions;
using SmartAudit.Api.Services;
using SmartAudit.Core.Domain;
using SmartAudit.Core.Domain.Enums;
using SmartAudit.Core.Domain.Helpers;

namespace SmartAudit.Api.Features.Facilities.LocatePlace
{
    // Místo se určí buď adresou z našeptávače (AddressKey), nebo bodem v mapě (Latitude + Longitude)
    public record LocatePlaceCommand(ViewerContext Viewer, string? AddressKey, double? Latitude, double? Longitude) : IRequest<Response<PlaceDto>>;

    public record PlaceDto(
        double Latitude,
        double Longitude,
        PlaceAddressDto? Address,
        PlaceTerritoryDto Territory,
        PlaceBuildingDto? Building,
        bool CadastreAvailable,
        HeritageStatus HeritageStatus,
        string? HeritageName,
        IReadOnlyCollection<PlaceNeighborDto> Neighbors);

    public record PlaceAddressDto(string Label, string? Street, string? HouseNumber, string? Psc);

    public record PlaceTerritoryDto(string MunicipalityCode, string MunicipalityName, string OrpName, string DistrictCode, string DistrictName);

    // Geometry = přesný půdorys z ČÚZK (GeoJSON); odhad podlaží podle kategorie, protože tu správce ještě může změnit
    public record PlaceBuildingDto(Geometry Footprint, double? BuiltUpArea, int? ConstructionYear, string? CurrentUse, string? RuianCode, EnergyCategory EnergyCategory, IReadOnlyDictionary<FacilityCategory, int> EstimatedFloors);

    public record PlaceNeighborDto(Guid Id, string Code, string Name, double DistanceMeters, bool SameBuilding, bool Own);

    public class LocatePlaceHandler : IRequestHandler<LocatePlaceCommand, Response<PlaceDto>>
    {
        private static readonly GeometryFactory Factory = new(new PrecisionModel(), 4326);

        private readonly IRuianGeocodingService _geocoding;
        private readonly IPlaceLookupService _placeLookup;

        public LocatePlaceHandler(IRuianGeocodingService geocoding, IPlaceLookupService placeLookup)
        {
            _geocoding = geocoding;
            _placeLookup = placeLookup;
        }

        public async Task<Response<PlaceDto>> Handle(LocatePlaceCommand request, CancellationToken ct)
        {
            RuianAddressPoint? address;
            Point location;

            try
            {
                if (!string.IsNullOrWhiteSpace(request.AddressKey))
                {
                    address = await _geocoding.ResolveAsync(request.AddressKey.Trim(), ct);

                    if (address == null)
                        return Response.NotFound<PlaceDto>("Adresu se v registru RÚIAN nepodařilo najít");

                    location = address.Location;
                }
                else
                {
                    if (request.Latitude is not (>= -90 and <= 90) || request.Longitude is not (>= -180 and <= 180))
                        return Response.ValidationFailed<PlaceDto>("Zadejte adresu nebo vyberte místo v mapě");

                    location = Factory.CreatePoint(new Coordinate(request.Longitude.Value, request.Latitude.Value));
                    address = await ReverseAsync(location, ct);
                }
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return Response.Unavailable<PlaceDto>("Registr adres ČÚZK teď neodpovídá. Zkuste to za chvíli, nebo vyberte místo v mapě.");
            }

            var place = await _placeLookup.LookupAsync(location, ct);

            if (place.Territory == null)
                return Response.ValidationFailed<PlaceDto>("Místo leží mimo Královéhradecký kraj — aplikace spravuje jen budovy v kraji");

            return Response.Success(ToDto(place, place.Territory, address, request.Viewer));
        }

        // Bod z mapy nemusí mít adresu (pole, park) — budova se dá přidat i bez ní
        private async Task<RuianAddressPoint?> ReverseAsync(Point location, CancellationToken ct)
        {
            try
            {
                return await _geocoding.ReverseAsync(location, ct);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return null;
            }
        }

        private static PlaceDto ToDto(PlaceContext place, PlaceTerritory territory, RuianAddressPoint? address, ViewerContext viewer)
        {
            var parsed = address == null ? null : RuianAddress.Parse(address.Address);
            var building = place.Building;

            return new PlaceDto(
                place.Location.Y,
                place.Location.X,
                address == null ? null : new PlaceAddressDto(address.Address, parsed!.Street, parsed.HouseNumber, parsed.Psc),
                new PlaceTerritoryDto(territory.MunicipalityCode, territory.MunicipalityName, territory.OrpName, territory.DistrictCode, territory.DistrictName),
                building == null
                    ? null
                    : new PlaceBuildingDto(
                        building.Footprint,
                        building.BuiltUpArea,
                        building.ConstructionYear,
                        building.CurrentUse,
                        building.RuianCode,
                        BuildingEstimator.CategoryFromYear(building.ConstructionYear),
                        Enum.GetValues<FacilityCategory>().ToDictionary(c => c, c => BuildingEstimator.EstimateFloors(building.BuiltUpArea, c))),
                place.CadastreAvailable,
                place.HeritageStatus,
                place.HeritageName,
                place.Neighbors
                    .Select(n => new PlaceNeighborDto(n.Id, n.Code, n.Name, n.DistanceMeters, n.SameBuilding, viewer.OrganizationId != null && n.OrganizationId == viewer.OrganizationId))
                    .ToList());
        }
    }
}
