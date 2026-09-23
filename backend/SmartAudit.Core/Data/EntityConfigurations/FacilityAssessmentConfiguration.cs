using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAudit.Core.Data.Tables;

namespace SmartAudit.Core.Data.EntityConfigurations
{
    public class FacilityAssessmentConfiguration : IEntityTypeConfiguration<FacilityAssessment>
    {
        public void Configure(EntityTypeBuilder<FacilityAssessment> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);

            builder.Property(a => a.Headline)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(a => a.Summary).HasMaxLength(2000);

            builder.Property(a => a.FindingsJson)
                .IsRequired()
                .HasColumnType("jsonb");

            builder.Property(a => a.AnnualLossCzk).HasPrecision(14, 2);

            builder.HasIndex(a => a.FacilityId).IsUnique();

            builder.HasIndex(a => a.Status);

            builder.HasOne(a => a.Facility)
                    .WithOne(f => f.Assessment)
                    .HasForeignKey<FacilityAssessment>(a => a.FacilityId)
                    .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
