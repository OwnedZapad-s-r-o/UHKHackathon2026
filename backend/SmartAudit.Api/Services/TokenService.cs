using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SmartAudit.Core.Common;
using SmartAudit.Core.Domain.Settings;

namespace SmartAudit.Api.Services
{
    public interface ITokenService
    {
        string GenerateToken(string userId, string userRole, Guid? organizationId);
    }
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;

        public TokenService(JwtSettings jwtSetting)
        {
            _jwtSettings = jwtSetting;
        }
        public string GenerateToken(string userId, string userRole, Guid? organizationId)
        {
            //claims - userId, role, organizace správce
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, userId),
                new(AppClaims.Role, userRole),
                new(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString())
            };

            if (organizationId != null)
                claims.Add(new Claim(AppClaims.OrganizationId, organizationId.Value.ToString()));

            //Nastavení klíče
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            //Vytvoření tokenu
            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
