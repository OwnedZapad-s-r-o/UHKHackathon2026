using SmartAudit.Core.Common;

namespace SmartAudit.Core.Data.Tables
{
    public class User
    {
        public Guid Id { get; private set; } = Guid.CreateVersion7();

        // Přihlášení
        public string Email { get; private set; } = string.Empty;
        public string PasswordHash { get; private set; } = string.Empty;
        public string DisplayName { get; private set; } = string.Empty;
        public string Role { get; private set; } = AppRoles.Public;
        public bool IsDemo { get; private set; }


        // Cizí klíče
        public Guid? OrganizationId { get; private set; }


        // Časové údaje
        public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;


        // Navigační vlastnosti
        public Organization? Organization { get; set; }

        private User() { }

        public User(string email, string passwordHash, string displayName, string role, Guid? organizationId = null, bool isDemo = false)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("E-mail nesmí být prázdný.", nameof(email));

            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentException("Heslo nesmí být prázdné.", nameof(passwordHash));

            if (!AppRoles.All.Contains(role))
                throw new ArgumentException("Neznámá role uživatele.", nameof(role));

            if (role == AppRoles.Manager && organizationId == null)
                throw new ArgumentException("Správce musí mít přiřazenou organizaci.", nameof(organizationId));

            Email = email.Trim().ToLowerInvariant();
            PasswordHash = passwordHash;
            DisplayName = displayName;
            Role = role;
            OrganizationId = organizationId;
            IsDemo = isDemo;
            CreatedAt = DateTimeOffset.UtcNow;
        }

        public void AssignOrganization(Guid organizationId)
        {
            OrganizationId = organizationId;
        }
    }
}
