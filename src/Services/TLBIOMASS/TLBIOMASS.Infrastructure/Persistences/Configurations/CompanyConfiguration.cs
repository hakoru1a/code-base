using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TLBIOMASS.Domain.Companies;

namespace TLBIOMASS.Infrastructure.Persistences.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("companies");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("ID");

        builder.Property(x => x.CompanyName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Address)
            .HasMaxLength(255);

        builder.Property(x => x.TaxCode)
            .HasMaxLength(50);

        builder.Property(x => x.Representative)
            .HasMaxLength(100);

        builder.Property(x => x.Position)
            .HasMaxLength(100);

        builder.Property(x => x.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(x => x.Email)
            .HasMaxLength(100);

        builder.Property(x => x.IdentityCardNo)
            .HasMaxLength(20);

        builder.Property(x => x.IssuePlace)
            .HasMaxLength(255);

        // Audit mappings
        builder.Property(x => x.CreatedDate).HasColumnName("Created_At");
        builder.Property(x => x.CreatedBy).HasColumnName("Created_By");
        builder.Property(x => x.LastModifiedDate).HasColumnName("Last_Updated_At");
        builder.Property(x => x.LastModifiedBy).HasColumnName("Last_Updated_By");
    }
}
