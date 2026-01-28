using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TLBIOMASS.Domain.WeighingTicketCancels;

namespace TLBIOMASS.Infrastructure.Persistences.Configurations;

public class WeighingTicketCancelConfiguration : IEntityTypeConfiguration<WeighingTicketCancel>
{
    public void Configure(EntityTypeBuilder<WeighingTicketCancel> builder)
    {
        builder.ToTable("weighing_ticket_cancels"); 
        builder.Ignore("Status");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("ID");
        builder.Property(x => x.WeighingTicketId).HasColumnName("WeighingTicketID");
        builder.Property(x => x.CancelReason).HasColumnName("CancelReason");
        builder.Property(x => x.CreatedDate).HasColumnName("Created_At");
        builder.Property(x => x.CreatedBy).HasColumnName("Created_By");
        builder.Property(x => x.LastModifiedDate).HasColumnName("Last_Updated_At");
        builder.Property(x => x.LastModifiedBy).HasColumnName("Last_Updated_By");
    }
}
