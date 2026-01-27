using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TLBIOMASS.Domain.Agencies;

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

        builder.Property(x => x.Phone)
            .HasMaxLength(50);

        builder.Property(x => x.Email)
            .HasMaxLength(100);

        builder.Property(x => x.Address)
            .HasMaxLength(255);

        builder.Property(x => x.BankAccount)
            .HasMaxLength(50);

        builder.Property(x => x.BankName)
            .HasMaxLength(20);

        builder.Property(x => x.IdentityCard)
            .HasMaxLength(50)
            .HasColumnName("IdentityCardNo");

        builder.Property(x => x.IssuePlace)
            .HasMaxLength(255)
            .HasColumnName("IssuePlace");

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true)
            .HasColumnName("IsActive");

        // Audit mappings
        builder.Property(x => x.CreatedDate).HasColumnName("Created_At");
        builder.Property(x => x.CreatedBy).HasColumnName("Created_By");
        builder.Property(x => x.LastModifiedDate).HasColumnName("Last_Updated_At");
        builder.Property(x => x.LastModifiedBy).HasColumnName("Last_Updated_By");
    }
}
