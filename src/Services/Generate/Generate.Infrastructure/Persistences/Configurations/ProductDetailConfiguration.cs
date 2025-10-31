using Generate.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Generate.Infrastructure.Persistences.Configurations
{
    public class ProductDetailConfiguration : IEntityTypeConfiguration<ProductDetail>
    {
        public void Configure(EntityTypeBuilder<ProductDetail> builder)
        {
            // Table name
            builder.ToTable("PRODUCT_DETAIL");

            // Primary key
            builder.HasKey(pd => pd.Id);

            // Properties
            builder.Property(pd => pd.ProductId)
                .IsRequired();

            builder.Property(pd => pd.Description)
                .HasMaxLength(1000);

            // Relationships
            // One-to-One with Product
            builder.HasOne(pd => pd.Product)
                .WithOne(p => p.ProductDetail)
                .HasForeignKey<ProductDetail>(pd => pd.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(pd => pd.ProductId)
                .IsUnique(); // Ensure one-to-one relationship

            builder.MapAuditColumns();
        }
    }
}
