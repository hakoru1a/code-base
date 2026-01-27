using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TLBIOMASS.Domain.Landowners;

namespace TLBIOMASS.Infrastructure.Persistences.Configurations;

public class LandownerConfiguration : IEntityTypeConfiguration<Landowner>
{
    public void Configure(EntityTypeBuilder<Landowner> builder)
    {
        builder.ToTable("landowners");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("OwnerID");

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(150)
            .HasColumnName("OwnerName");

        builder.Property(x => x.Phone)
            .HasMaxLength(50)
            .HasColumnName("Phone");

        builder.Property(x => x.Email)
            .HasMaxLength(100)
            .HasColumnName("Email");

        builder.Property(x => x.Address)
            .HasMaxLength(255)
            .HasColumnName("Address");

        builder.Property(x => x.BankAccount)
            .HasMaxLength(50)
            .HasColumnName("BankAccount");

        builder.Property(x => x.BankName)
            .HasMaxLength(20)
            .HasColumnName("BankName");

        builder.Property(x => x.IdentityCardNo)
            .HasMaxLength(20)
            .HasColumnName("IdentityCardNo");

        builder.Property(x => x.IssuePlace)
            .HasMaxLength(255)
            .HasColumnName("IssuePlace");

        builder.Property(x => x.IssueDate)
            .HasColumnName("IssueDate");

        builder.Property(x => x.DateOfBirth)
            .HasColumnType("date")
            .HasColumnName("DateOfBirth");

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
