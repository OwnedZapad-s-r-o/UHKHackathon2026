using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAudit.Core.Data.Tables;

namespace SmartAudit.Core.Data.EntityConfigurations
{
    public class FacilityContractConfiguration : IEntityTypeConfiguration<FacilityContract>
    {
        public void Configure(EntityTypeBuilder<FacilityContract> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.ContractId)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(c => c.VersionId)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(c => c.Title)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(c => c.PartnerName)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(c => c.ContractNumber).HasMaxLength(200);
            builder.Property(c => c.PartnerIco).HasMaxLength(20);
            builder.Property(c => c.AmountWithoutVat).HasPrecision(16, 2);
            builder.Property(c => c.AmountWithVat).HasPrecision(16, 2);
            builder.Property(c => c.Category).HasConversion<string>().HasMaxLength(30);
            builder.Property(c => c.Commodity).HasConversion<string>().HasMaxLength(20);
            builder.Property(c => c.PriceSource).HasMaxLength(500);

            builder.Property(c => c.AttachmentsJson)
                .IsRequired()
                .HasColumnType("jsonb");

            // ID smlouvy je v registru celostátně jedinečné — nová verze přepíše stejný řádek
            builder.HasIndex(c => c.ContractId).IsUnique();

            builder.HasIndex(c => new { c.OrganizationId, c.Category });

            builder.HasOne(c => c.Organization)
                    .WithMany(o => o.Contracts)
                    .HasForeignKey(c => c.OrganizationId)
                    .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
