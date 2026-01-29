using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TLBIOMASS.Domain.Companies;
using Shared.Domain.ValueObjects;
using TLBIOMASS.Domain.Companies.ValueObjects;

namespace TLBIOMASS.Infrastructure.Persistences.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("companies");
        builder.Property(x => x.Status)
            .HasConversion<int>()
            .HasColumnName("Status");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("ID");

        builder.Property(x => x.CompanyName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.TaxCode)
            .HasMaxLength(50);

        builder.OwnsOne(x => x.Representative, r =>
        {
            r.Property(x => x.Name).HasMaxLength(100).HasColumnName("Representative");
            r.Property(x => x.Position).HasMaxLength(100).HasColumnName("Position");
        });

        builder.OwnsOne(x => x.Contact, c =>
        {
            c.Property(x => x.Phone).HasMaxLength(20).HasColumnName("PhoneNumber");
            c.Property(x => x.Email).HasMaxLength(100).HasColumnName("Email");
            c.Property(x => x.Address).HasMaxLength(255).HasColumnName("Address");
        });

        builder.OwnsOne(x => x.Identity, i =>
        {
            i.Property(x => x.IdentityNumber).HasMaxLength(20).HasColumnName("IdentityCardNo");
            i.Property(x => x.IssuePlace).HasMaxLength(255).HasColumnName("IssuePlace");
            i.Property(x => x.IssueDate).HasColumnName("IssueDate");
        });

        // Audit mappings
        builder.Property(x => x.CreatedDate).HasColumnName("Created_At");
        builder.Property(x => x.CreatedBy).HasColumnName("Created_By");
        builder.Property(x => x.LastModifiedDate).HasColumnName("Last_Updated_At");
        builder.Property(x => x.LastModifiedBy).HasColumnName("Last_Updated_By");
    }
}
