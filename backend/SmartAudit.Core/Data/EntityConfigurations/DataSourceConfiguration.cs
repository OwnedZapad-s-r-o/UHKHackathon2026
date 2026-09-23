using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAudit.Core.Data.Tables;

namespace SmartAudit.Core.Data.EntityConfigurations
{
    public class DataSourceConfiguration : IEntityTypeConfiguration<DataSource>
    {
        public void Configure(EntityTypeBuilder<DataSource> builder)
        {
            builder.HasKey(d => d.Id);

            builder.Property(d => d.Key)
                .IsRequired()
                .HasMaxLength(64);

            builder.Property(d => d.Title)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(d => d.Provider).HasMaxLength(100);
            builder.Property(d => d.DatasetId).HasMaxLength(64);
            builder.Property(d => d.ServiceUrl).HasMaxLength(1000);
            builder.Property(d => d.PortalUrl).HasMaxLength(1000);
            builder.Property(d => d.Group).HasMaxLength(100);
            builder.Property(d => d.Purpose).HasMaxLength(1000);
            builder.Property(d => d.LastStatus).HasConversion<string>().HasMaxLength(20);

            builder.HasIndex(d => d.Key).IsUnique();
        }
    }
}
