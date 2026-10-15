using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAudit.Core.Data.Tables;

namespace SmartAudit.Core.Data.EntityConfigurations
{
    public class MunicipalityContactConfiguration : IEntityTypeConfiguration<MunicipalityContact>
    {
        public void Configure(EntityTypeBuilder<MunicipalityContact> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.MunicipalityCode)
                .IsRequired()
                .HasMaxLength(6);

            builder.Property(c => c.MunicipalityName).HasMaxLength(100);
            builder.Property(c => c.DataBox).HasMaxLength(20);
            builder.Property(c => c.Email).HasMaxLength(200);
            builder.Property(c => c.Phone).HasMaxLength(50);
            builder.Property(c => c.Web).HasMaxLength(300);
            builder.Property(c => c.Address).HasMaxLength(300);

            builder.HasIndex(c => c.MunicipalityCode).IsUnique();
        }
    }
}
