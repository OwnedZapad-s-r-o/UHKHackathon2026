using MediatR;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Buffer;
using SmartAudit.Core.Data;
using SmartAudit.Core.Domain;

namespace SmartAudit.Api.Features.Facilities.GetFootprint
{
    // Buffered = obrys rozšířený pro 3D zvýraznění v mapě; bez něj přesný půdorys z ČÚZK (otevřená data)
    public record GetFootprintCommand(Guid FacilityId, bool Buffered = false) : IRequest<Response<GetFootprintDto>>;

    // GeoJSON Feature — geometrii serializuje GeoJsonConverterFactory z Program.cs; bez půdorysu je Geometry null
    public record GetFootprintDto(string Type, Geometry? Geometry, FootprintPropertiesDto Properties);

    public record FootprintPropertiesDto(Guid FacilityId, double HeightMeters, int Floors, bool HeightEstimated, double? BuiltUpArea, int? ConstructionYear, string? RuianCode, string Source);

    // Půdorys budovy z ČÚZK. Katastr je veřejný, proto bez kontroly role — a bez stavu auditu,
    // aby šla odpověď cachovat (stav se mění každých pár minut, půdorys skoro nikdy)
    public class GetFootprintHandler : IRequestHandler<GetFootprintCommand, Response<GetFootprintDto>>
    {
        // ČÚZK výšku budov nevede — odhad podlaží × 3,3 m
        private const double FloorHeightMeters = 3.3;

        // Obrys se zvětší o ~0,4 m: stejnou budovu kreslí i podkladová mapa (OSM má v ČR půdorysy z katastru)
        // a barevná budova ji musí celou obalit, jinak se stěny překrývají a barva prosvítá jen místy
        private const double OutlineBufferDegrees = 0.0000036;

        private const string Source = "ČÚZK — INSPIRE Budovy (RÚIAN)";

        private static readonly BufferParameters OutlineBuffer = new() { JoinStyle = JoinStyle.Mitre, QuadrantSegments = 1 };

        private readonly SmartAuditDbContext _dbContext;

        public GetFootprintHandler(SmartAuditDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Response<GetFootprintDto>> Handle(GetFootprintCommand request, CancellationToken ct)
        {
            var facility = await _dbContext.Facilities
                .AsNoTracking()
                .Where(f => f.Id == request.FacilityId)
                .Select(f => new
                {
                    Footprint = f.Building != null ? f.Building.Footprint : null,
                    Floors = f.Building != null ? f.Building.EstimatedFloors : 2,
                    BuiltUpArea = f.Building != null ? f.Building.BuiltUpArea : null,
                    ConstructionYear = f.Building != null ? f.Building.ConstructionYear : null,
                    RuianCode = f.Building != null ? f.Building.RuianCode : null,
                })
                .FirstOrDefaultAsync(ct);

            if (facility == null)
                return Response.NotFound<GetFootprintDto>("Objekt s tímto id neexistuje");

            var properties = new FootprintPropertiesDto(
                request.FacilityId,
                facility.Floors * FloorHeightMeters,
                facility.Floors,
                true,
                facility.BuiltUpArea,
                facility.ConstructionYear,
                facility.RuianCode,
                Source);

            var geometry = request.Buffered ? facility.Footprint?.Buffer(OutlineBufferDegrees, OutlineBuffer) : facility.Footprint;

            return Response.Success(new GetFootprintDto("Feature", geometry, properties));
        }
    }
}
