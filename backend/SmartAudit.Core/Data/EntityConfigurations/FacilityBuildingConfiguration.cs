using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAudit.Core.Data.Tables;

namespace SmartAudit.Core.Data.EntityConfigurations
{
    public class FacilityBuildingConfiguration : IEntityTypeConfiguration<FacilityBuilding>
    {
        public void Configure(EntityTypeBuilder<FacilityBuilding> builder)
        {
            builder.HasKey(b => b.Id);

            builder.Property(b => b.Footprint).HasColumnType("geometry(Geometry, 4326)");
            builder.Property(b => b.CurrentUse).HasMaxLength(100);
            builder.Property(b => b.Condition).HasMaxLength(100);
            builder.Property(b => b.ParcelRef).HasMaxLength(300);
            builder.Property(b => b.RuianCode).HasMaxLength(20);
            builder.Property(b => b.EnergyCategory).HasConversion<string>().HasMaxLength(20);

            builder.HasIndex(b => b.FacilityId).IsUnique();

            builder.HasOne(b => b.Facility)
                    .WithOne(f => f.Building)
                    .HasForeignKey<FacilityBuilding>(b => b.FacilityId)
                    .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
