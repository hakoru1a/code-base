using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TLBIOMASS.Domain.Receivers;

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

        builder.Property(x => x.Phone)
            .HasMaxLength(50);

        builder.Property(x => x.BankAccount)
            .HasMaxLength(100);

        builder.Property(x => x.BankName)
            .HasMaxLength(255);

        builder.Property(x => x.IdentityNumber)
            .HasMaxLength(20);

        builder.Property(x => x.IssuedPlace)
            .HasMaxLength(255);

        builder.Property(x => x.Address)
            .HasMaxLength(500);

        builder.Property(x => x.IsDefault)
            .HasDefaultValue(false);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.Property(x => x.Note)
            .HasColumnType("text");
            
        builder.Property(x => x.CreatedAt)
            .IsRequired();
    }
}
