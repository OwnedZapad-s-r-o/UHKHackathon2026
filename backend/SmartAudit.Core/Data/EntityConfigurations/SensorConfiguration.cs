using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAudit.Core.Data.Tables;

namespace SmartAudit.Core.Data.EntityConfigurations
{
    public class SensorConfiguration : IEntityTypeConfiguration<Sensor>
    {
        public void Configure(EntityTypeBuilder<Sensor> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.Metric).HasConversion<string>().HasMaxLength(20);

            builder.HasIndex(s => s.FacilityId);

            builder.HasIndex(s => s.ZoneId);

            builder.HasOne(s => s.Facility)
                    .WithMany(f => f.Sensors)
                    .HasForeignKey(s => s.FacilityId)
                    .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(s => s.Zone)
                    .WithMany(z => z.Sensors)
                    .HasForeignKey(s => s.ZoneId)
                    .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
