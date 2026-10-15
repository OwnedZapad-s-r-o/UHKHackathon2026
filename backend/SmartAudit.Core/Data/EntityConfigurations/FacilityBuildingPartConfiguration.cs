using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAudit.Core.Data.Tables;

namespace SmartAudit.Core.Data.EntityConfigurations
{
    public class FacilityBuildingPartConfiguration : IEntityTypeConfiguration<FacilityBuildingPart>
    {
        public void Configure(EntityTypeBuilder<FacilityBuildingPart> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Footprint)
                .IsRequired()
                .HasColumnType("geometry(Polygon, 4326)");

            builder.Property(p => p.Source)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(p => p.FacilityId);

            builder.HasOne(p => p.Facility)
                    .WithMany()
                    .HasForeignKey(p => p.FacilityId)
                    .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
