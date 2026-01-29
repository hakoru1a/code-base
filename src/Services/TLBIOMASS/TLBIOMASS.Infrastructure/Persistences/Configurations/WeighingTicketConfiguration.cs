using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TLBIOMASS.Domain.WeighingTickets;
using TLBIOMASS.Domain.Payments;
using TLBIOMASS.Domain.Receivers;
using TLBIOMASS.Domain.WeighingTickets.ValueObjects;

namespace TLBIOMASS.Infrastructure.Persistences.Configurations;

public class WeighingTicketConfiguration : IEntityTypeConfiguration<WeighingTicket>
{
    public void Configure(EntityTypeBuilder<WeighingTicket> builder)
    {
        builder.ToTable("phieucan"); 
        builder.Property(x => x.Status)
            .HasConversion<int>()
            .HasColumnName("Status");
            
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.TicketNumber)
            .HasColumnName("so_phieu")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.CustomerId)
            .HasColumnName("khach_hang_id")
            .IsRequired();

        builder.Property(x => x.MaterialId)
            .HasColumnName("hang_hoa_id")
            .IsRequired();

        builder.HasOne(x => x.Material)
            .WithMany()
            .HasForeignKey(x => x.MaterialId);

        builder.Property(x => x.VehiclePlate)
            .HasColumnName("bien_so_xe")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.TicketType)
            .HasColumnName("loai_phieu")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.CustomerName)
            .HasColumnName("ten_khach_hang")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.MaterialName)
            .HasColumnName("ten_hang_hoa")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.PhoneNumber)
            .HasColumnName("so_dien_thoai")
            .HasMaxLength(20);

        builder.OwnsOne(x => x.Weights, w =>
        {
            w.Property(x => x.WeightIn).HasColumnName("trong_luong_vao");
            w.Property(x => x.WeightOut).HasColumnName("trong_luong_ra");
            w.Property(x => x.NetWeight).HasColumnName("trong_luong_hang"); 
            w.Property(x => x.ImpurityDeduction).HasColumnName("tru_tap_chat");
            w.Property(x => x.PayableWeight).HasColumnName("trong_luong_thanh_toan");
        });
        
        builder.Property(x => x.Price)
            .HasColumnName("don_gia")
            .HasColumnType("bigint");

        builder.Property(x => x.TotalAmount)
            .HasColumnName("thanh_tien")
            .HasColumnType("bigint");

        builder.Property(x => x.FirstWeighingTime)
            .HasColumnName("thoi_gian_can_1")
            .HasColumnType("datetime");

        builder.Property(x => x.SecondWeighingTime)
            .HasColumnName("thoi_gian_can_2")
            .HasColumnType("datetime");

        builder.Property(x => x.PaymentDate)
            .HasColumnName("ngay_thanh_toan")
            .HasColumnType("date");

        builder.Property(x => x.CreatedDate)
            .HasColumnName("ngay_tao")
            .HasColumnType("datetime");

        builder.Property(x => x.FSCClassification)
            .HasColumnName("phan_loai_fsc")
            .HasMaxLength(255);

        builder.Property(x => x.HasOriginProfile)
            .HasColumnName("ho_so_nguon_goc")
            .HasColumnType("tinyint(1)");

        builder.OwnsOne(x => x.Images, i =>
        {
            i.Property(x => x.VehicleFrontImage).HasColumnName("anh_dau_xe").HasMaxLength(500);
            i.Property(x => x.VehicleBodyImage).HasColumnName("anh_than_xe").HasMaxLength(500);
            i.Property(x => x.VehicleRearImage).HasColumnName("anh_duoi_xe").HasMaxLength(500);
        });

        builder.Property(x => x.Note)
            .HasColumnName("ghi_chu")
            .HasColumnType("text");

        builder.Property(x => x.QualityStatus)
            .HasColumnName("quality_status")
            .HasMaxLength(20);

        builder.Property(x => x.CreatedByString)
            .HasColumnName("created_by")
            .HasMaxLength(50);
            
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp");

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp");

        builder.Property(x => x.ReceiverId)
            .HasColumnName("nguoi_nhan_id");

        // Relationships
        builder.HasOne(x => x.FinalPayment)
            .WithOne()
            .HasForeignKey<WeighingTicketPayment>(x => x.WeighingTicketId);

        builder.HasMany(x => x.PaymentDetails)
            .WithOne(x => x.WeighingTicket)
            .HasForeignKey(x => x.WeighingTicketId);

        builder.HasOne(x => x.Receiver)
            .WithMany()
            .HasForeignKey(x => x.ReceiverId);
    }
}
