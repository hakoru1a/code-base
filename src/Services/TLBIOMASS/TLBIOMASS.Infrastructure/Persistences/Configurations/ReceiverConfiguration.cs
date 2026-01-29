using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TLBIOMASS.Domain.Receivers;
using Shared.Domain.ValueObjects;

namespace TLBIOMASS.Infrastructure.Persistences.Configurations;

public class ReceiverConfiguration : IEntityTypeConfiguration<Receiver>
{
    public void Configure(EntityTypeBuilder<Receiver> builder)
    {
        builder.ToTable("receivers");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.OwnsOne(x => x.Contact, c =>
        {
            c.Property(x => x.Phone).HasMaxLength(50).HasColumnName("Phone");
            c.Property(x => x.Email).HasMaxLength(255).HasColumnName("Email");
            c.Property(x => x.Address).HasMaxLength(500).HasColumnName("Address");

            c.Property(x => x.Note).HasColumnType("text").HasColumnName("Note");
        });


        builder.OwnsOne(x => x.Identity, i =>
        {
            i.Property(x => x.IdentityNumber).HasMaxLength(20).HasColumnName("IdentityNumber");
            i.Property(x => x.IssuePlace).HasMaxLength(255).HasColumnName("IssuedPlace");
            i.Property(x => x.IssueDate).HasColumnName("IssuedDate");
            i.Property(x => x.DateOfBirth).HasColumnName("DateOfBirth");
        });

        builder.Property(x => x.IsDefault)
            .HasDefaultValue(false);

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .HasColumnName("Status");

        builder.Property(x => x.CreatedDate).HasColumnName("CreatedAt");
        builder.Property(x => x.CreatedBy).HasColumnName("Created_By");
        builder.Property(x => x.LastModifiedDate).HasColumnName("UpdatedAt");
        builder.Property(x => x.LastModifiedBy).HasColumnName("Last_Updated_By");

        builder.HasMany(x => x.BankAccounts)
            .WithOne()
            .HasForeignKey(x => x.OwnerId);

        builder.Navigation(x => x.BankAccounts)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
