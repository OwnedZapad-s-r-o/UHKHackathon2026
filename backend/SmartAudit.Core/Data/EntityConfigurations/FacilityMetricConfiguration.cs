using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAudit.Core.Data.Tables;

namespace SmartAudit.Core.Data.EntityConfigurations
{
    public class FacilityMetricConfiguration : IEntityTypeConfiguration<FacilityMetric>
    {
        public void Configure(EntityTypeBuilder<FacilityMetric> builder)
        {
            builder.HasKey(m => m.Id);

            builder.Property(m => m.SchoolYear)
                .IsRequired()
                .HasMaxLength(9);

            builder.HasIndex(m => new { m.FacilityId, m.SchoolYear }).IsUnique();

            builder.HasOne(m => m.Facility)
                    .WithMany(f => f.Metrics)
                    .HasForeignKey(m => m.FacilityId)
                    .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
