using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAudit.Api.Extentions;
using SmartAudit.Api.Services.Sensors;
using SmartAudit.Core.Common;
using SmartAudit.Core.Data;
using SmartAudit.Core.Data.Tables;
using SmartAudit.Core.Domain;
using SmartAudit.Core.Domain.Helpers;
using SmartAudit.Core.Domain.Settings;

namespace SmartAudit.Api.Features.Sensors.ClaimGateway
{
    // Tělo požadavku — přihlášeného uživatele doplní controller
    public record ClaimGatewayRequest(string Serial, string Password, Guid FacilityId, string? Name);

    public record ClaimGatewayCommand(ViewerContext Viewer, string Serial, string Password, Guid FacilityId, string? Name) : IRequest<Response<ClaimGatewayDto>>;

    public record ClaimGatewayDto(Guid Id, string GatewayId);

    // Připojení nové gatewaye heslem z nálepky (PairingProtocol): aplikace heslem zašifruje nabídku s přihlašovacími údaji
    // k brokeru a gateway ji buď rozšifruje (správné heslo), nebo odmítne. Heslo se neukládá ani neloguje.
    public class ClaimGatewayHandler : IRequestHandler<ClaimGatewayCommand, Response<ClaimGatewayDto>>
    {
        private static readonly TimeSpan ReplyTimeout = TimeSpan.FromSeconds(10);

        private readonly SmartAuditDbContext _dbContext;
        private readonly IPairingRegistry _registry;
        private readonly IIqrfCommandBus _bus;
        private readonly IIqrfGatewayState _state;
        private readonly IqrfSettings _settings;

        public ClaimGatewayHandler(SmartAuditDbContext dbContext, IPairingRegistry registry, IIqrfCommandBus bus, IIqrfGatewayState state, IqrfSettings settings)
        {
            _dbContext = dbContext;
            _registry = registry;
            _bus = bus;
            _state = state;
            _settings = settings;
        }

        public async Task<Response<ClaimGatewayDto>> Handle(ClaimGatewayCommand request, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(_settings.ProvisioningPassword) || string.IsNullOrEmpty(_settings.DeviceSecret))
                return Response.Unavailable<ClaimGatewayDto>("Párování heslem není na tomto serveru zapnuté. Připojte gateway ručně.");

            if (!PairingProtocol.IsValidSerial(request.Serial))
                return Response.ValidationFailed<ClaimGatewayDto>("Neplatné sériové číslo zařízení");

            if (PairingCode.Normalize(request.Password).Length == 0)
                return Response.ValidationFailed<ClaimGatewayDto>("Zadejte heslo z nálepky");

            var serial = request.Serial.ToUpperInvariant();
            var now = DateTimeOffset.UtcNow;

            var facility = await _dbContext.Facilities
                .AsNoTracking()
                .Select(f => new { f.Id, f.Name, f.OrganizationId })
                .FirstOrDefaultAsync(f => f.Id == request.FacilityId, ct);

            if (facility == null)
                return Response.NotFound<ClaimGatewayDto>("Objekt s tímto id neexistuje");

            if (request.Viewer.Role == AppRoles.Manager && facility.OrganizationId != request.Viewer.OrganizationId)
                return Response.Forbidden<ClaimGatewayDto>("K tomuto nemáte oprávnění");

            var gatewayId = PairableDevices.GatewayIdOf(serial);
            if (await _dbContext.SensorDevices.AnyAsync(d => d.GatewayId == gatewayId, ct))
                return Response.ValidationFailed<ClaimGatewayDto>("Tahle gateway už je připojená");

            var discovered = _registry.Find(serial, now);
            if (discovered == null)
                return Response.NotFound<ClaimGatewayDto>("Zařízení teď není online. Zkontrolujte, že je zapnuté a připojené k síti.");

            if (_registry.BlockedFor(serial, now) is { } blocked)
                return Response.ValidationFailed<ClaimGatewayDto>($"Příliš mnoho špatných hesel. Zkuste to znovu za {Math.Max(1, (int)Math.Ceiling(blocked.TotalMinutes))} min.");

            var offer = PairingProtocol.SealOffer(request.Password, serial, GatewayCredentials.For(_settings.DeviceSecret, gatewayId));
            var reply = await _bus.RequestAsync(new OutgoingMessage(PairingProtocol.OfferTopic(serial), offer), IqrfCommandBus.PairingKey(serial), ReplyTimeout, ct);

            if (reply == null)
                return Response.Unavailable<ClaimGatewayDto>("Zařízení neodpovědělo. Je zapnuté a připojené k síti? Zkuste to za chvíli znovu.");

            if (PairingProtocol.ParseResult(reply) != true)
            {
                _registry.RecordFailure(serial, now);
                return Response.ValidationFailed<ClaimGatewayDto>("Heslo nesouhlasí. Opište ho z nálepky na spodní straně gatewaye.");
            }

            var announcement = discovered.Announcement;
            var name = string.IsNullOrWhiteSpace(request.Name) ? $"Gateway {announcement.Model} · {serial}" : request.Name.Trim();
            var emulated = announcement.Emulated && PairableDevices.FindGateway(serial) != null;

            // Emulovaná gateway odpovídá hned — kratší interval, aby demo bylo živé (jako u testovací sítě)
            var device = new SensorDevice(facility.Id, name[..Math.Min(name.Length, 100)], gatewayId, emulated ? 10 : 15, announcement.Nodes.Select(n => n.Address).ToArray(), isVirtual: emulated);
            _dbContext.SensorDevices.Add(device);

            await _dbContext.SaveChangesAsync(ct);

            _registry.Forget(serial);
            _registry.ResetFailures(serial);
            _state.ForgetUnknown(gatewayId);

            return Response.Success(new ClaimGatewayDto(device.Id, device.GatewayId));
        }
    }
}
