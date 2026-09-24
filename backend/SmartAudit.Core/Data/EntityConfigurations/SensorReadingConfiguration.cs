using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAudit.Core.Data.Tables;

namespace SmartAudit.Core.Data.EntityConfigurations
{
    public class SensorReadingConfiguration : IEntityTypeConfiguration<SensorReading>
    {
        public void Configure(EntityTypeBuilder<SensorReading> builder)
        {
            // Složený klíč pokrývá i nejčastější dotaz: senzor + časový rozsah.
            // Tabulka je v databázi rozdělená na měsíční oddíly podle Timestamp (migrace PartitionSensorReadings,
            // nové oddíly zakládá ReadingPartitionService) — klíč proto musí čas obsahovat.
            builder.HasKey(r => new { r.SensorId, r.Timestamp });

            // BRIN: měření přibývají v čase, index je o řády menší než B-tree a na časové okno stačí
            builder.HasIndex(r => r.Timestamp).HasMethod("brin");

            builder.HasOne<Sensor>()
                    .WithMany()
                    .HasForeignKey(r => r.SensorId)
                    .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
