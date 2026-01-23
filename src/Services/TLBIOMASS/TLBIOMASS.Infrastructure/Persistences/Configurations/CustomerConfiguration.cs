using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TLBIOMASS.Domain.Customers;

namespace TLBIOMASS.Infrastructure.Persistences.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        // Table name
        builder.ToTable("khachhang");

        // Primary key
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // Properties
        builder.Property(c => c.TenKhachHang)
            .HasColumnName("ten_khach_hang")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.DienThoai)
            .HasColumnName("dien_thoai")
            .HasMaxLength(20);

        builder.Property(c => c.DiaChi)
            .HasColumnName("dia_chi")
            .HasMaxLength(500);

        builder.Property(c => c.GhiChu)
            .HasColumnName("ghi_chu")
            .HasColumnType("text");

        builder.Property(c => c.Email)
            .HasColumnName("email")
            .HasMaxLength(100);

        builder.Property(c => c.MaSoThue)
            .HasColumnName("ma_so_thue")
            .HasMaxLength(50);

        builder.Property(c => c.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp");

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp");

        // Indexes
        builder.HasIndex(c => c.TenKhachHang)
            .HasDatabaseName("idx_khachhang_ten");

        builder.HasIndex(c => c.MaSoThue)
            .HasDatabaseName("idx_khachhang_mst");

        builder.HasIndex(c => c.IsActive)
            .HasDatabaseName("idx_khachhang_active");
    }
}
