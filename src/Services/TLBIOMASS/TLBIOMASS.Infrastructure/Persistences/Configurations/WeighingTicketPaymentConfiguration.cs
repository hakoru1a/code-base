using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TLBIOMASS.Domain.Payments;

namespace TLBIOMASS.Infrastructure.Persistences.Configurations;

public class WeighingTicketPaymentConfiguration : IEntityTypeConfiguration<WeighingTicketPayment>
{
    public void Configure(EntityTypeBuilder<WeighingTicketPayment> builder)
    {
        builder.ToTable("weighing_ticket_payments");
        builder.Property(x => x.Status)
            .HasConversion<int>()
            .HasColumnName("Status");
        
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("ID");
        
        builder.Property(x => x.UnitPrice)
            .HasColumnType("decimal(18,2)");
            
        builder.Property(x => x.TotalPayableAmount)
            .HasColumnType("decimal(18,2)");
            
        builder.Property(x => x.Note)
            .HasMaxLength(500);

        builder.Property(x => x.WeighingTicketId)
            .HasColumnName("WeighingTicketID")
            .IsRequired();

        builder.HasIndex(x => x.WeighingTicketId)
            .IsUnique();

         builder.Property(x => x.CreatedDate).HasColumnName("Created_At");
        builder.Property(x => x.CreatedBy).HasColumnName("Created_By");
        builder.Property(x => x.LastModifiedDate).HasColumnName("Last_Updated_At");
        builder.Property(x => x.LastModifiedBy).HasColumnName("Last_Updated_By");
    }
}
