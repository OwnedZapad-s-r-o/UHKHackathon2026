using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAudit.Core.Data.Tables;

namespace SmartAudit.Core.Data.EntityConfigurations
{
    public class SensorDeviceConfiguration : IEntityTypeConfiguration<SensorDevice>
    {
        public void Configure(EntityTypeBuilder<SensorDevice> builder)
        {
            builder.HasKey(d => d.Id);

            builder.Property(d => d.Name).IsRequired().HasMaxLength(100);

            builder.Property(d => d.GatewayId).IsRequired().HasMaxLength(64);

            builder.HasIndex(d => d.GatewayId).IsUnique();

            builder.HasIndex(d => d.FacilityId);

            builder.HasOne(d => d.Facility)
                    .WithMany()
                    .HasForeignKey(d => d.FacilityId)
                    .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
