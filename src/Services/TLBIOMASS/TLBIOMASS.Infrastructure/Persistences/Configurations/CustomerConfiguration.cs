using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TLBIOMASS.Domain.Customers;
using Shared.Domain.ValueObjects;

namespace TLBIOMASS.Infrastructure.Persistences.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("khachhang");
        builder.Ignore("Status");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(c => c.Name)
            .HasColumnName("ten_khach_hang")
            .HasMaxLength(200)
            .IsRequired();

        builder.OwnsOne(c => c.Contact, c =>
        {
            c.Property(x => x.Phone).HasColumnName("dien_thoai").HasMaxLength(20);
            c.Property(x => x.Address).HasColumnName("dia_chi").HasMaxLength(500);
            c.Property(x => x.Note).HasColumnName("ghi_chu").HasColumnType("text");
            c.Property(x => x.Email).HasColumnName("email").HasMaxLength(100);
        });

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

        builder.HasIndex(c => c.Name)
            .HasDatabaseName("idx_khachhang_ten");

        builder.HasIndex(c => c.TaxCode)
            .HasDatabaseName("idx_khachhang_mst");

        builder.HasIndex(c => c.IsActive)
            .HasDatabaseName("idx_khachhang_active");
    }
}
