using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAudit.Core.Data.Tables;

namespace SmartAudit.Core.Data.EntityConfigurations
{
    public class DataImportConfiguration : IEntityTypeConfiguration<DataImport>
    {
        public void Configure(EntityTypeBuilder<DataImport> builder)
        {
            builder.HasKey(i => i.Id);

            builder.Property(i => i.Status).HasConversion<string>().HasMaxLength(20);
            builder.Property(i => i.Error).HasMaxLength(2000);

            builder.HasIndex(i => i.DataSourceId);

            builder.HasOne(i => i.DataSource)
                    .WithMany(d => d.Imports)
                    .HasForeignKey(i => i.DataSourceId)
                    .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
