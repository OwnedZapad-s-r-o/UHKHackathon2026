using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAudit.Core.Data.Tables;

namespace SmartAudit.Core.Data.EntityConfigurations
{
    public class FacilityConfiguration : IEntityTypeConfiguration<Facility>
    {
        public void Configure(EntityTypeBuilder<Facility> builder)
        {
            builder.HasKey(f => f.Id);

            builder.Property(f => f.SourceKey)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(f => f.Code)
                .IsRequired()
                .HasMaxLength(32);

            builder.Property(f => f.Name)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(f => f.Category).HasConversion<string>().HasMaxLength(20);
            builder.Property(f => f.HeritageStatus).HasConversion<string>().HasMaxLength(20);
            builder.Property(f => f.SimulationProfile).HasConversion<string>().HasMaxLength(30);

            builder.Property(f => f.Redizo).HasMaxLength(9);
            builder.Property(f => f.Ico).HasMaxLength(8);
            builder.Property(f => f.Web).HasMaxLength(300);
            builder.Property(f => f.Street).HasMaxLength(200);
            builder.Property(f => f.HouseNumber).HasMaxLength(20);
            builder.Property(f => f.Psc).HasMaxLength(10);
            builder.Property(f => f.MunicipalityCode).HasMaxLength(6);
            builder.Property(f => f.MunicipalityName).HasMaxLength(100);
            builder.Property(f => f.OrpCode).HasMaxLength(4);
            builder.Property(f => f.OrpName).HasMaxLength(100);
            builder.Property(f => f.DistrictCode).HasMaxLength(6);
            builder.Property(f => f.DistrictName).HasMaxLength(100);
            builder.Property(f => f.HeritageName).HasMaxLength(300);

            builder.Property(f => f.Location)
                .IsRequired()
                .HasColumnType("geometry(Point, 4326)");

            builder.HasIndex(f => f.SourceKey).IsUnique();

            builder.HasIndex(f => f.Code);

            builder.HasIndex(f => f.OrganizationId);

            builder.HasIndex(f => f.Location).HasMethod("gist");

            builder.HasOne(f => f.Organization)
                    .WithMany(o => o.Facilities)
                    .HasForeignKey(f => f.OrganizationId)
                    .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
