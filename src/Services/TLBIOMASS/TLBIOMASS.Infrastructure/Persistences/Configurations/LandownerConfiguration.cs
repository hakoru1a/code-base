using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TLBIOMASS.Domain.Landowners;
using Shared.Domain.ValueObjects;

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

        builder.OwnsOne(x => x.Contact, c =>
        {
            c.Property(x => x.Phone).HasMaxLength(50).HasColumnName("Phone");
            c.Property(x => x.Email).HasMaxLength(100).HasColumnName("Email");
            c.Property(x => x.Address).HasMaxLength(255).HasColumnName("Address");
            c.Ignore(x => x.Note);
        });


        builder.OwnsOne(x => x.Identity, i =>
        {
            i.Property(x => x.IdentityNumber).HasMaxLength(20).HasColumnName("IdentityCardNo");
            i.Property(x => x.IssuePlace).HasMaxLength(255).HasColumnName("IssuePlace");
            i.Property(x => x.IssueDate).HasColumnName("IssueDate");
            i.Property(x => x.DateOfBirth).HasColumnType("date").HasColumnName("DateOfBirth");
        });

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true)
            .HasColumnName("IsActive");

        // Audit mappings
        builder.Property(x => x.CreatedDate).HasColumnName("Created_At");
        builder.Property(x => x.CreatedBy).HasColumnName("Created_By");
        builder.Property(x => x.LastModifiedDate).HasColumnName("Last_Updated_At");
        builder.Property(x => x.LastModifiedBy).HasColumnName("Last_Updated_By");
        builder.Ignore("Status");

        builder.HasMany(x => x.BankAccounts)
            .WithOne()
            .HasForeignKey(x => x.OwnerId);
    }
}
