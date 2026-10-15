using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAudit.Core.Data.Tables;

namespace SmartAudit.Core.Data.EntityConfigurations
{
    public class FacilityPreAuditConfiguration : IEntityTypeConfiguration<FacilityPreAudit>
    {
        public void Configure(EntityTypeBuilder<FacilityPreAudit> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);

            builder.Property(p => p.Profile).HasConversion<string>().HasMaxLength(30);

            builder.Property(p => p.DominantDriver).HasMaxLength(100);

            builder.Property(p => p.ParametersJson)
                .IsRequired()
                .HasColumnType("jsonb");

            builder.Property(p => p.FindingsJson)
                .IsRequired()
                .HasColumnType("jsonb");

            builder.Property(p => p.AnnualHeatCostCzk).HasPrecision(14, 2);
            builder.Property(p => p.HeatPriceCzkPerKwh).HasPrecision(10, 3);
            builder.Property(p => p.RegulationPotentialCzk).HasPrecision(14, 2);
            builder.Property(p => p.IqrfHardwareCostCzk).HasPrecision(12, 2);

            builder.HasIndex(p => p.FacilityId).IsUnique();

            builder.HasIndex(p => p.Score);

            builder.HasOne(p => p.Facility)
                    .WithOne(f => f.PreAudit)
                    .HasForeignKey<FacilityPreAudit>(p => p.FacilityId)
                    .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
