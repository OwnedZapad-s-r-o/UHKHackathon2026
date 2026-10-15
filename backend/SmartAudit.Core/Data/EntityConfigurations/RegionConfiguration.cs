using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAudit.Core.Data.Tables;

namespace SmartAudit.Core.Data.EntityConfigurations
{
    public class RegionConfiguration : IEntityTypeConfiguration<Region>
    {
        public void Configure(EntityTypeBuilder<Region> builder)
        {
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Type).HasConversion<string>().HasMaxLength(10);

            builder.Property(r => r.Code)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(r => r.OrpCode).HasMaxLength(4);
            builder.Property(r => r.ThreatLevel).HasMaxLength(100);
            builder.Property(r => r.GrantTotal).HasPrecision(16, 2);
            builder.Property(r => r.IndividualGrantTotal).HasPrecision(16, 2);
            builder.Property(r => r.ProgramGrantTotal).HasPrecision(16, 2);
            builder.Property(r => r.Geometry).HasColumnType("geometry(Geometry, 4326)");

            builder.HasIndex(r => new { r.Type, r.Code }).IsUnique();

            builder.HasIndex(r => r.Geometry).HasMethod("gist");
        }
    }
}
