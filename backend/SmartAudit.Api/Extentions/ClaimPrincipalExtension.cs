using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using SmartAudit.Core.Common;

namespace SmartAudit.Api.Extentions
{
    public static class ClaimPrincipalExtension
    {
        public static bool TryGetUserId(this ClaimsPrincipal principal, out Guid userId)
        {
            userId = Guid.Empty;

            var rawUserId = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);

            return Guid.TryParse(rawUserId, out userId);
        }

        public static string? GetRole(this ClaimsPrincipal principal)
        {
            return principal.FindFirstValue(AppClaims.Role);
        }

        public static Guid? GetOrganizationId(this ClaimsPrincipal principal)
        {
            return Guid.TryParse(principal.FindFirstValue(AppClaims.OrganizationId), out var organizationId) ? organizationId : null;
        }

        // Nepřihlášený návštěvník se chová jako veřejnost
        public static ViewerContext GetViewer(this ClaimsPrincipal principal)
        {
            var role = principal.GetRole() ?? AppRoles.Public;

            return new ViewerContext(role, principal.GetOrganizationId());
        }
    }

    public record ViewerContext(string Role, Guid? OrganizationId)
    {
        public bool CanSeeMoney => Role is AppRoles.Region or AppRoles.Manager;
    }
}
