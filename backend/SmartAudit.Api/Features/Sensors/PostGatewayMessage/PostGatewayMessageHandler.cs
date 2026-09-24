using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAudit.Api.Extentions;
using SmartAudit.Api.Services.Sensors;
using SmartAudit.Core.Common;
using SmartAudit.Core.Data;
using SmartAudit.Core.Domain;

namespace SmartAudit.Api.Features.Sensors.PostGatewayMessage
{
    public record PostGatewayMessageCommand(ViewerContext Viewer, string GatewayId, string Payload) : IRequest<Response<PostGatewayMessageDto>>;

    public record PostGatewayMessageDto(bool KnownGateway, string MessageType, int Readings, int Stored);

    // Záložní vstup přes HTTP: stejná odpověď IQRF JSON API, jakou gateway posílá do MQTT
    // (např. přeposlaná z Node-RED na gatewayi, nebo ručně při testu bez brokeru)
    public class PostGatewayMessageHandler : IRequestHandler<PostGatewayMessageCommand, Response<PostGatewayMessageDto>>
    {
        private readonly SmartAuditDbContext _dbContext;
        private readonly ISensorIngestService _ingestService;

        public PostGatewayMessageHandler(SmartAuditDbContext dbContext, ISensorIngestService ingestService)
        {
            _dbContext = dbContext;
            _ingestService = ingestService;
        }

        public async Task<Response<PostGatewayMessageDto>> Handle(PostGatewayMessageCommand request, CancellationToken ct)
        {
            var owner = await _dbContext.SensorDevices
                .AsNoTracking()
                .Where(d => d.GatewayId == request.GatewayId)
                .Select(d => new { d.Facility.OrganizationId })
                .FirstOrDefaultAsync(ct);

            if (owner == null)
                return Response.NotFound<PostGatewayMessageDto>("Gateway s tímto id není v aplikaci připojená");

            if (request.Viewer.Role == AppRoles.Manager && owner.OrganizationId != request.Viewer.OrganizationId)
                return Response.Forbidden<PostGatewayMessageDto>("K tomuto nemáte oprávnění");

            var result = await _ingestService.IngestAsync(request.GatewayId, request.Payload, ct);
            if (result == null)
                return Response.ValidationFailed<PostGatewayMessageDto>("Zpráva není odpověď IQRF JSON API (chybí mType)");

            return Response.Success(new PostGatewayMessageDto(result.KnownGateway, result.MessageType, result.Readings, result.Stored));
        }
    }
}
