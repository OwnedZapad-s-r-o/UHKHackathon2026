using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAudit.Core.Data.Tables;

namespace SmartAudit.Core.Data.EntityConfigurations
{
    public class ZoneConfiguration : IEntityTypeConfiguration<Zone>
    {
        public void Configure(EntityTypeBuilder<Zone> builder)
        {
            builder.HasKey(z => z.Id);

            builder.Property(z => z.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(z => z.Wing).HasMaxLength(50);
            builder.Property(z => z.Type).HasConversion<string>().HasMaxLength(20);

            builder.HasIndex(z => z.FacilityId);

            builder.HasOne(z => z.Facility)
                    .WithMany(f => f.Zones)
                    .HasForeignKey(z => z.FacilityId)
                    .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
