using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAudit.Core.Data.Tables;

namespace SmartAudit.Core.Data.EntityConfigurations
{
    public class DemoDeviceConfiguration : IEntityTypeConfiguration<DemoDevice>
    {
        public void Configure(EntityTypeBuilder<DemoDevice> builder)
        {
            builder.HasKey(d => d.Id);

            builder.Property(d => d.Kind).HasConversion<string>().HasMaxLength(20);

            builder.Property(d => d.Behavior).HasConversion<string>().HasMaxLength(20);

            builder.Property(d => d.Serial).IsRequired().HasMaxLength(8);

            builder.Property(d => d.Model).IsRequired().HasMaxLength(40);

            builder.Property(d => d.Code).IsRequired().HasMaxLength(9);

            builder.Property(d => d.ParentGatewayId).HasMaxLength(64);

            builder.HasIndex(d => d.Serial).IsUnique();

            builder.HasIndex(d => d.ParentGatewayId);
        }
    }
}
