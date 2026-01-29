using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TLBIOMASS.Domain.BankAccounts;

namespace TLBIOMASS.Infrastructure.Persistences.Configurations;

public class BankAccountConfiguration : IEntityTypeConfiguration<BankAccount>
{
    public void Configure(EntityTypeBuilder<BankAccount> builder)
    {
        builder.ToTable("bank_accounts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.BankName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.AccountNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.OwnerType)
            .IsRequired()
            .HasMaxLength(50)
            .HasConversion<string>();

        builder.Property(x => x.OwnerId)
            .IsRequired();

        builder.Property(x => x.IsDefault)
            .HasDefaultValue(false);

        builder.HasIndex(x => new { x.OwnerType, x.OwnerId })
            .HasDatabaseName("idx_bank_accounts_owner");

        builder.Property(x => x.CreatedDate).HasColumnName("Created_At");
        builder.Property(x => x.CreatedBy).HasColumnName("Created_By");
        builder.Property(x => x.LastModifiedDate).HasColumnName("Last_Updated_At");
        builder.Property(x => x.LastModifiedBy).HasColumnName("Last_Updated_By");

        builder.Ignore("Status");
    }
}
