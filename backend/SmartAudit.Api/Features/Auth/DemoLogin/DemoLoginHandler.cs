using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAudit.Api.Services;
using SmartAudit.Core.Common;
using SmartAudit.Core.Data;
using SmartAudit.Core.Domain;

namespace SmartAudit.Api.Features.Auth.DemoLogin
{
    public record DemoLoginCommand(string Role) : IRequest<Response<DemoLoginResponse>>;

    public record DemoLoginResponse(string Token, Guid UserId, string DisplayName, string Role, Guid? OrganizationId, string? OrganizationName);

    // Tlačítko „Demo“ — přihlásí předpřipraveného uživatele zvolené role bez hesla
    public class DemoLoginHandler : IRequestHandler<DemoLoginCommand, Response<DemoLoginResponse>>
    {
        private readonly SmartAuditDbContext _dbContext;
        private readonly ITokenService _tokenService;

        public DemoLoginHandler(SmartAuditDbContext dbContext, ITokenService tokenService)
        {
            _dbContext = dbContext;
            _tokenService = tokenService;
        }

        public async Task<Response<DemoLoginResponse>> Handle(DemoLoginCommand request, CancellationToken ct)
        {
            if (!AppRoles.All.Contains(request.Role))
                return Response.ValidationFailed<DemoLoginResponse>("Neznámá role. Povolené jsou Kraj, Spravce a Verejnost.");

            var user = await _dbContext.Users
                .AsNoTracking()
                .Where(u => u.IsDemo && u.Role == request.Role)
                .Select(u => new { u.Id, u.DisplayName, u.Role, u.OrganizationId, OrganizationName = u.Organization != null ? u.Organization.Name : null })
                .FirstOrDefaultAsync(ct);

            if (user == null)
                return Response.NotFound<DemoLoginResponse>("Demo účet se ještě připravuje, zkuste to za chvíli.");

            var token = _tokenService.GenerateToken(user.Id.ToString(), user.Role, user.OrganizationId);

            var dto = new DemoLoginResponse(token, user.Id, user.DisplayName, user.Role, user.OrganizationId, user.OrganizationName);

            return Response.Success(dto);
        }
    }
}
