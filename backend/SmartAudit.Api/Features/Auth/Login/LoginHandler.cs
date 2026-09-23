using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAudit.Api.Services;
using SmartAudit.Core.Data;
using SmartAudit.Core.Domain;
using SmartAudit.Core.Domain.Helpers;

namespace SmartAudit.Api.Features.Auth.Login
{
    public record LoginCommand(string Email, string Password) : IRequest<Response<LoginResponse>>;

    public record LoginResponse(string Token, Guid UserId, string DisplayName, string Role, Guid? OrganizationId);

    public class LoginHandler : IRequestHandler<LoginCommand, Response<LoginResponse>>
    {
        private readonly SmartAuditDbContext _dbContext;
        private readonly ITokenService _tokenService;

        public LoginHandler(SmartAuditDbContext dbContext, ITokenService tokenService)
        {
            _dbContext = dbContext;
            _tokenService = tokenService;
        }

        public async Task<Response<LoginResponse>> Handle(LoginCommand request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return Response.ValidationFailed<LoginResponse>("Neplatné přihlašovací údaje");

            var email = request.Email.Trim().ToLowerInvariant();

            var user = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email, ct);

            if (user == null || user.IsDemo || !PasswordHasher.Verify(request.Password, user.PasswordHash))
                return Response.NotFound<LoginResponse>("Neplatné přihlašovací údaje");

            var token = _tokenService.GenerateToken(user.Id.ToString(), user.Role, user.OrganizationId);

            var dto = new LoginResponse(token, user.Id, user.DisplayName, user.Role, user.OrganizationId);

            return Response.Success(dto);
        }
    }
}
