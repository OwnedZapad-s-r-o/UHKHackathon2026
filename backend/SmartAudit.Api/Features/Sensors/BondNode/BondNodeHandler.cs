using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAudit.Api.Extentions;
using SmartAudit.Api.Services.Sensors;
using SmartAudit.Core.Common;
using SmartAudit.Core.Data;
using SmartAudit.Core.Domain;
using SmartAudit.Core.Domain.Helpers;
using SmartAudit.Core.Domain.Settings;

namespace SmartAudit.Api.Features.Sensors.BondNode
{
    public record BondNodeRequest(string Code);

    public record BondNodeCommand(ViewerContext Viewer, Guid GatewayId, string Code) : IRequest<Response<BondNodeDto>>;

    public record BondNodeDto(int Address);

    // Přidání čidla do sítě gatewaye kódem z nálepky (IQRF SmartConnect). Odpověď gatewaye zpracuje ingest — uloží novou adresu
    // uzlu — a teprve pak se vrátí sem; při dalším dotazu gateway načte senzory nového uzlu a objeví se k přiřazení do místnosti.
    public class BondNodeHandler : IRequestHandler<BondNodeCommand, Response<BondNodeDto>>
    {
        // Bonding v síti IQMESH trvá i několik sekund
        private static readonly TimeSpan ReplyTimeout = TimeSpan.FromSeconds(15);

        private readonly SmartAuditDbContext _dbContext;
        private readonly IIqrfCommandBus _bus;
        private readonly IPairingRegistry _registry;
        private readonly IqrfSettings _settings;

        public BondNodeHandler(SmartAuditDbContext dbContext, IIqrfCommandBus bus, IPairingRegistry registry, IqrfSettings settings)
        {
            _dbContext = dbContext;
            _bus = bus;
            _registry = registry;
            _settings = settings;
        }

        public async Task<Response<BondNodeDto>> Handle(BondNodeCommand request, CancellationToken ct)
        {
            // Krátký kód z nálepky (XXXX-XXXX) se sjednotí, dlouhý naskenovaný IQRF Code jde gatewayi beze změny
            var normalized = PairingCode.Normalize(request.Code);
            if (normalized.Length < 8 || request.Code.Trim().Length > 200)
                return Response.ValidationFailed<BondNodeDto>("Zadejte kód z nálepky čidla (8 znaků) nebo naskenovaný IQRF Code");

            var code = normalized.Length == 8 ? PairingCode.Format(normalized) : request.Code.Trim();

            var device = await _dbContext.SensorDevices
                .AsNoTracking()
                .Select(d => new { d.Id, d.GatewayId, d.Facility.OrganizationId })
                .FirstOrDefaultAsync(d => d.Id == request.GatewayId, ct);

            if (device == null)
                return Response.NotFound<BondNodeDto>("Gateway s tímto id neexistuje");

            if (request.Viewer.Role == AppRoles.Manager && device.OrganizationId != request.Viewer.OrganizationId)
                return Response.Forbidden<BondNodeDto>("K tomuto nemáte oprávnění");

            var now = DateTimeOffset.UtcNow;
            var attemptsKey = $"nodes:{device.Id}";
            if (_registry.BlockedFor(attemptsKey, now) is { } blocked)
                return Response.ValidationFailed<BondNodeDto>($"Příliš mnoho špatných kódů. Zkuste to znovu za {Math.Max(1, (int)Math.Ceiling(blocked.TotalMinutes))} min.");

            var messageId = $"sa-{Guid.NewGuid():N}"[..16];
            var message = new OutgoingMessage(
                string.Format(_settings.RequestTopicTemplate, device.GatewayId),
                IqrfResponseParser.SmartConnectRequest(code, messageId),
                device.GatewayId,
                IqrfResponseParser.SmartConnect);

            var reply = await _bus.RequestAsync(message, IqrfCommandBus.MessageKey(messageId), ReplyTimeout, ct);
            if (reply == null)
                return Response.Unavailable<BondNodeDto>("Gateway neodpověděla. Je online? Zkuste to za chvíli znovu.");

            var response = IqrfResponseParser.Parse(reply);
            if (response is not { IsOk: true, AssignedAddress: { } address })
            {
                _registry.RecordFailure(attemptsKey, now);
                return Response.ValidationFailed<BondNodeDto>("Čidlo se nepřipojilo. Zkontrolujte kód z nálepky a že je čidlo zapnuté v dosahu gatewaye.");
            }

            _registry.ResetFailures(attemptsKey);

            return Response.Success(new BondNodeDto(address));
        }
    }
}
