using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAudit.Core.Data.Tables;

namespace SmartAudit.Core.Data.EntityConfigurations
{
    public class SensorReadingConfiguration : IEntityTypeConfiguration<SensorReading>
    {
        public void Configure(EntityTypeBuilder<SensorReading> builder)
        {
            // Složený klíč pokrývá i nejčastější dotaz: senzor + časový rozsah
            builder.HasKey(r => new { r.SensorId, r.Timestamp });

            builder.HasIndex(r => r.Timestamp);

            builder.HasOne<Sensor>()
                    .WithMany()
                    .HasForeignKey(r => r.SensorId)
                    .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
