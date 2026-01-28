using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TLBIOMASS.Domain.Agencies;
using Shared.Domain.ValueObjects;

namespace TLBIOMASS.Infrastructure.Persistences.Configurations;

public class AgencyConfiguration : IEntityTypeConfiguration<Agency>
{
    public void Configure(EntityTypeBuilder<Agency> builder)
    {
        builder.ToTable("agencies");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.OwnsOne(x => x.Contact, c =>
        {
            c.Property(x => x.Phone).HasMaxLength(50).HasColumnName("Phone");
            c.Property(x => x.Email).HasMaxLength(100).HasColumnName("Email");
            c.Property(x => x.Address).HasMaxLength(255).HasColumnName("Address");
        });

        builder.OwnsOne(x => x.Bank, b =>
        {
            b.Property(x => x.BankAccount).HasMaxLength(50).HasColumnName("BankAccount");
            b.Property(x => x.BankName).HasMaxLength(20).HasColumnName("BankName");
        });

        builder.OwnsOne(x => x.Identity, i =>
        {
            i.Property(x => x.IdentityNumber).HasMaxLength(50).HasColumnName("IdentityCard");
            i.Property(x => x.IssuePlace).HasMaxLength(255).HasColumnName("IssuePlace");
            i.Property(x => x.IssueDate).HasColumnName("IssueDate");
        });

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true)
            .HasColumnName("IsActive");

        builder.Property(x => x.CreatedDate).HasColumnName("Created_At");
        builder.Property(x => x.CreatedBy).HasColumnName("Created_By");
        builder.Property(x => x.LastModifiedDate).HasColumnName("Last_Updated_At");
        builder.Property(x => x.LastModifiedBy).HasColumnName("Last_Updated_By");
    }
}
