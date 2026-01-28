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
        
        builder.OwnsOne(x => x.PaymentAmount, money =>
        {
            money.Property(p => p.Amount)
                .HasColumnName("Amount")
                .HasColumnType("decimal(18,2)");

            money.Property(p => p.RemainingAmount)
                .HasColumnName("RemainingAmount")
                .HasColumnType("decimal(18,2)");
        });

        builder.OwnsOne(x => x.Info, info =>
        {
            info.Property(p => p.PaymentCode).HasColumnName("PaymentCode").HasMaxLength(50);
            info.Property(p => p.Note).HasColumnName("Note");
            info.Property(p => p.PaymentDate)
                .HasColumnName("PaymentDate")
                .HasColumnType("datetime");
            info.Property(p => p.CustomerPaymentDate)
                .HasColumnName("CustomerPaymentDate")
                .HasColumnType("datetime")
                .IsRequired(false);
            
            info.HasIndex(p => p.PaymentCode);
        });

        builder.Property(x => x.AgencyId)
            .HasColumnName("AgencyID")
            .IsRequired(false);

        builder.OwnsOne(x => x.ProcessStatus, status =>
        {
            status.Property(p => p.IsPaid).HasColumnName("IsPaid");
            status.Property(p => p.IsLocked).HasColumnName("IsLocked");
        });

        // Relationships
        builder.HasIndex(x => x.WeighingTicketId);
        
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
