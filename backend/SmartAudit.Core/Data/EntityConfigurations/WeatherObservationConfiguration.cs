using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAudit.Core.Data.Tables;

namespace SmartAudit.Core.Data.EntityConfigurations
{
    public class WeatherObservationConfiguration : IEntityTypeConfiguration<WeatherObservation>
    {
        public void Configure(EntityTypeBuilder<WeatherObservation> builder)
        {
            builder.HasKey(w => new { w.OrpCode, w.Timestamp });

            builder.Property(w => w.OrpCode).HasMaxLength(4);
        }
    }
}
