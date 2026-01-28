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
        builder.Property(c => c.Name)
            .HasColumnName("ten_khach_hang")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.Phone)
            .HasColumnName("dien_thoai")
            .HasMaxLength(20);

        builder.Property(c => c.Address)
            .HasColumnName("dia_chi")
            .HasMaxLength(500);

        builder.Property(c => c.Note)
            .HasColumnName("ghi_chu")
            .HasColumnType("text");

        builder.Property(c => c.Email)
            .HasColumnName("email")
            .HasMaxLength(100);

        builder.Property(c => c.TaxCode)
            .HasColumnName("ma_so_thue")
            .HasMaxLength(50);

        builder.Property(c => c.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(c => c.CreatedDate)
            .HasColumnName("created_at")
            .HasColumnType("timestamp");

        builder.Property(c => c.LastModifiedDate)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp");

        builder.Ignore(c => c.CreatedBy);
        builder.Ignore(c => c.LastModifiedBy);

        // Indexes
        builder.HasIndex(c => c.Name)
            .HasDatabaseName("idx_khachhang_ten");

        builder.HasIndex(c => c.TaxCode)
            .HasDatabaseName("idx_khachhang_mst");

        builder.HasIndex(c => c.IsActive)
            .HasDatabaseName("idx_khachhang_active");
    }
}
