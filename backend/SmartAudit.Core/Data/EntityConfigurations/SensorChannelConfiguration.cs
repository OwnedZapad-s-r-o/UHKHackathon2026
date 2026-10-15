using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAudit.Core.Data.Tables;

namespace SmartAudit.Core.Data.EntityConfigurations
{
    public class SensorChannelConfiguration : IEntityTypeConfiguration<SensorChannel>
    {
        public void Configure(EntityTypeBuilder<SensorChannel> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.ChannelKey).IsRequired().HasMaxLength(40);

            builder.Property(c => c.Name).IsRequired().HasMaxLength(80);

            builder.Property(c => c.Unit).HasMaxLength(20);

            builder.Property(c => c.Metric).HasConversion<string>().HasMaxLength(20);

            builder.HasIndex(c => new { c.DeviceId, c.ChannelKey }).IsUnique();

            builder.HasOne(c => c.Device)
                    .WithMany(d => d.Channels)
                    .HasForeignKey(c => c.DeviceId)
                    .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(c => c.Sensor)
                    .WithMany()
                    .HasForeignKey(c => c.SensorId)
                    .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
