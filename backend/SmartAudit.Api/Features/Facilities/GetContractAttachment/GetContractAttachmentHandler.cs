using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAudit.Api.Extentions;
using SmartAudit.Api.Services.Contracts;
using SmartAudit.Core.Common;
using SmartAudit.Core.Data;
using SmartAudit.Core.Domain;
using SmartAudit.Core.Domain.Helpers;

namespace SmartAudit.Api.Features.Facilities.GetContractAttachment
{
    public record GetContractAttachmentCommand(Guid FacilityId, Guid ContractId, string AttachmentId, ViewerContext Viewer) : IRequest<Response<GetContractAttachmentDto>>;

    public record GetContractAttachmentDto(Stream Content, string FileName);

    // Originální PDF smlouvy z Registru smluv přes naše API — registr posílá soubor jako přílohu se zákazem rámců
    // (X-Frame-Options: SAMEORIGIN), takže náhled v inspekčním panelu jinak nejde. Stahuje se jen příloha,
    // kterou máme u smlouvy organizace uloženou — žádná libovolná adresa z požadavku.
    public class GetContractAttachmentHandler : IRequestHandler<GetContractAttachmentCommand, Response<GetContractAttachmentDto>>
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly SmartAuditDbContext _dbContext;
        private readonly IContractRegisterService _registry;

        public GetContractAttachmentHandler(SmartAuditDbContext dbContext, IContractRegisterService registry)
        {
            _dbContext = dbContext;
            _registry = registry;
        }

        public async Task<Response<GetContractAttachmentDto>> Handle(GetContractAttachmentCommand request, CancellationToken ct)
        {
            var facility = await _dbContext.Facilities
                .AsNoTracking()
                .Where(f => f.Id == request.FacilityId)
                .Select(f => new { f.OrganizationId })
                .FirstOrDefaultAsync(ct);

            if (facility == null)
                return Response.NotFound<GetContractAttachmentDto>("Objekt s tímto id neexistuje");

            if (request.Viewer.Role == AppRoles.Manager && facility.OrganizationId != request.Viewer.OrganizationId)
                return Response.Forbidden<GetContractAttachmentDto>("K tomuto nemáte oprávnění");

            var attachmentsJson = await _dbContext.FacilityContracts
                .AsNoTracking()
                .Where(c => c.Id == request.ContractId && c.OrganizationId == facility.OrganizationId)
                .Select(c => c.AttachmentsJson)
                .FirstOrDefaultAsync(ct);

            if (attachmentsJson == null)
                return Response.NotFound<GetContractAttachmentDto>("Smlouva s tímto id u objektu neexistuje");

            var attachment = (JsonSerializer.Deserialize<List<RegistryAttachment>>(attachmentsJson, JsonOptions) ?? [])
                .FirstOrDefault(a => a.Id == request.AttachmentId);

            if (attachment == null)
                return Response.NotFound<GetContractAttachmentDto>("Příloha s tímto id u smlouvy neexistuje");

            Stream? content;
            try
            {
                content = await _registry.DownloadAttachmentAsync(attachment.Url, ct);
            }
            catch (Exception ex) when (ex is HttpRequestException || (ex is TaskCanceledException && !ct.IsCancellationRequested))
            {
                content = null;
            }

            if (content == null)
                return Response.Unavailable<GetContractAttachmentDto>("Registr smluv teď soubor nevydal, zkuste to prosím za chvíli.");

            return Response.Success(new GetContractAttachmentDto(content, attachment.Name));
        }
    }
}
