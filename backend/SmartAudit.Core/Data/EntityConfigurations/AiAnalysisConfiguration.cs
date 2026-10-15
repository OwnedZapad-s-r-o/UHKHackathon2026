using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAudit.Core.Data.Tables;

namespace SmartAudit.Core.Data.EntityConfigurations
{
    public class AiAnalysisConfiguration : IEntityTypeConfiguration<AiAnalysis>
    {
        public void Configure(EntityTypeBuilder<AiAnalysis> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Model)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(a => a.Summary)
                .IsRequired()
                .HasMaxLength(4000);

            builder.Property(a => a.RootCause).HasMaxLength(2000);
            builder.Property(a => a.LetterDraft).HasMaxLength(6000);

            builder.Property(a => a.RecommendationsJson)
                .IsRequired()
                .HasColumnType("jsonb");

            // Nejčastější dotaz: poslední analýza objektu
            builder.HasIndex(a => new { a.FacilityId, a.CreatedAt });

            builder.HasOne(a => a.Facility)
                    .WithMany()
                    .HasForeignKey(a => a.FacilityId)
                    .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
