using Microsoft.EntityFrameworkCore;
using SmartAudit.Core.Common;
using SmartAudit.Core.Data;
using SmartAudit.Core.Data.Tables;
using SmartAudit.Core.Domain.Enums;
using SmartAudit.Core.Domain.Helpers;

namespace SmartAudit.Api.Services
{
    public interface IDemoUserService
    {
        Task SeedAsync(CancellationToken ct);
    }

    // Uživatelé pro tlačítko „Demo“ — jeden na roli, heslo je náhodné (demo přihlášení heslo nepotřebuje)
    public class DemoUserService : IDemoUserService
    {
        private readonly SmartAuditDbContext _dbContext;

        public DemoUserService(SmartAuditDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task SeedAsync(CancellationToken ct)
        {
            var existing = await _dbContext.Users.Where(u => u.IsDemo).ToDictionaryAsync(u => u.Role, ct);

            if (!existing.ContainsKey(AppRoles.Region))
                _dbContext.Users.Add(new User("kraj@demo.smartaudit.cz", RandomHash(), "J. Novotný · Odbor investic", AppRoles.Region, isDemo: true));

            if (!existing.ContainsKey(AppRoles.Public))
                _dbContext.Users.Add(new User("verejnost@demo.smartaudit.cz", RandomHash(), "Rodič / veřejnost", AppRoles.Public, isDemo: true));

            // Správce dostane školu s víkendovým přetápěním, ať je v demu co ukázat
            var managerOrganizationId = await _dbContext.Facilities
                .Where(f => f.OrganizationId != null && f.Category == FacilityCategory.School)
                .OrderByDescending(f => f.SimulationProfile == SimulationProfile.WeekendOverheating)
                .ThenBy(f => f.Code)
                .Select(f => f.OrganizationId)
                .FirstOrDefaultAsync(ct);

            if (managerOrganizationId != null)
            {
                if (existing.TryGetValue(AppRoles.Manager, out var manager))
                    manager.AssignOrganization(managerOrganizationId.Value);
                else
                    _dbContext.Users.Add(new User("spravce@demo.smartaudit.cz", RandomHash(), "Ředitel školy · správce budovy", AppRoles.Manager, managerOrganizationId, isDemo: true));
            }

            await _dbContext.SaveChangesAsync(ct);
        }

        private static string RandomHash()
        {
            return PasswordHasher.HashPassword(Guid.NewGuid().ToString("N"));
        }
    }
}
