using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TLBIOMASS.Domain.Payments;

namespace TLBIOMASS.Infrastructure.Persistences.Configurations;

public class PaymentDetailConfiguration : IEntityTypeConfiguration<PaymentDetail>
{
    public void Configure(EntityTypeBuilder<PaymentDetail> builder)
    {
        builder.ToTable("payment_details");
        
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("ID");

        builder.Property(x => x.WeighingTicketId).HasColumnName("WeighingTicketID");
        
        builder.Property(x => x.Amount)
            .HasColumnName("Amount")
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.RemainingAmount)
            .HasColumnName("RemainingAmount")
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.Note).HasColumnName("Note");
        builder.Property(x => x.AgencyId)
            .HasColumnName("AgencyID")
            .IsRequired(false);

        builder.Property(x => x.IsPaid).HasColumnName("IsPaid");
        builder.Property(x => x.PaymentCode).HasColumnName("PaymentCode").HasMaxLength(50);
        builder.Property(x => x.IsLocked).HasColumnName("IsLocked");

        builder.Property(x => x.PaymentDate)
            .HasColumnName("PaymentDate")
            .HasColumnType("datetime");

        builder.Property(x => x.CustomerPaymentDate)
            .HasColumnName("CustomerPaymentDate")
            .HasColumnType("datetime")
            .IsRequired(false);

        builder.HasIndex(x => x.WeighingTicketId);
        builder.HasIndex(x => x.PaymentCode);

        // Relationships
        builder.HasOne(x => x.WeighingTicket)
            .WithMany(x => x.PaymentDetails)
            .HasForeignKey(x => x.WeighingTicketId);

        builder.HasOne(x => x.Agency)
            .WithMany()
            .HasForeignKey(x => x.AgencyId)
            .IsRequired(false);

        // Audit fields
        builder.Property(x => x.CreatedDate).HasColumnName("Created_At");
        builder.Property(x => x.CreatedBy).HasColumnName("Created_By");
        builder.Property(x => x.LastModifiedDate).HasColumnName("Last_Updated_At");
        builder.Property(x => x.LastModifiedBy).HasColumnName("Last_Updated_By");
    }
}
