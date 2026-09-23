using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAudit.Core.Data.Tables;

namespace SmartAudit.Core.Data.EntityConfigurations
{
    public class OpenDataFeatureConfiguration : IEntityTypeConfiguration<OpenDataFeature>
    {
        public void Configure(EntityTypeBuilder<OpenDataFeature> builder)
        {
            builder.HasKey(f => f.Id);

            builder.Property(f => f.ExternalId)
                .IsRequired()
                .HasMaxLength(64);

            builder.Property(f => f.Properties)
                .IsRequired()
                .HasColumnType("jsonb");

            builder.Property(f => f.Geometry).HasColumnType("geometry(Geometry, 4326)");

            builder.HasIndex(f => new { f.DataSourceId, f.ExternalId }).IsUnique();

            builder.HasIndex(f => f.Geometry).HasMethod("gist");

            builder.HasOne(f => f.DataSource)
                    .WithMany()
                    .HasForeignKey(f => f.DataSourceId)
                    .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
