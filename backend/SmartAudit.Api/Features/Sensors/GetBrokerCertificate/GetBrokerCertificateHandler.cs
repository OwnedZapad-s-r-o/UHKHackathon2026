using MediatR;
using SmartAudit.Core.Domain;
using SmartAudit.Core.Domain.Settings;

namespace SmartAudit.Api.Features.Sensors.GetBrokerCertificate
{
    public record GetBrokerCertificateCommand : IRequest<Response<BrokerCertificateDto>>;

    public record BrokerCertificateDto(byte[] Content, string FileName);

    // Veřejný certifikát certifikační autority brokeru — správce ho nahraje do IQRF gatewaye,
    // aby gateway ověřila, že se připojuje ke skutečnému brokeru kraje (TLS). Soukromý klíč broker nikdy nesdílí.
    public class GetBrokerCertificateHandler : IRequestHandler<GetBrokerCertificateCommand, Response<BrokerCertificateDto>>
    {
        private readonly IqrfSettings _settings;

        public GetBrokerCertificateHandler(IqrfSettings settings)
        {
            _settings = settings;
        }

        public async Task<Response<BrokerCertificateDto>> Handle(GetBrokerCertificateCommand request, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(_settings.CaCertificatePath) || !File.Exists(_settings.CaCertificatePath))
                return Response.NotFound<BrokerCertificateDto>("Certifikát brokeru zatím není k dispozici");

            var content = await File.ReadAllBytesAsync(_settings.CaCertificatePath, ct);

            return Response.Success(new BrokerCertificateDto(content, "smartaudit-ca.crt"));
        }
    }
}
