using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAudit.Core.Data.Tables;

namespace SmartAudit.Core.Data.EntityConfigurations
{
    public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
    {
        public void Configure(EntityTypeBuilder<Organization> builder)
        {
            builder.HasKey(o => o.Id);

            builder.Property(o => o.Ico)
                .IsRequired()
                .HasMaxLength(8);

            builder.Property(o => o.Name)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(o => o.Redizo).HasMaxLength(9);
            builder.Property(o => o.LegalForm).HasMaxLength(100);
            builder.Property(o => o.Department).HasMaxLength(100);
            builder.Property(o => o.RelationType).HasMaxLength(100);
            builder.Property(o => o.Web).HasMaxLength(300);
            builder.Property(o => o.PhotoUrl).HasMaxLength(1000);

            builder.HasIndex(o => o.Ico).IsUnique();

            builder.HasIndex(o => o.Redizo);
        }
    }
}
