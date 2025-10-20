using Generate.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Generate.Infrastructure.Persistences.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            // Table name
            builder.ToTable("PRODUCT");

            // Primary key
            builder.HasKey(p => p.Id);

            // Properties
            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.CategoryId)
                .IsRequired(false);

            // Relationships
            // One-to-Many with Category
            builder.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            // One-to-One with ProductDetail
            builder.HasOne(p => p.ProductDetail)
                .WithOne(pd => pd.Product)
                .HasForeignKey<ProductDetail>(pd => pd.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // One-to-Many with OrderItems
            builder.HasMany(p => p.OrderItems)
                .WithOne(oi => oi.Product)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deletion if product is in orders

            // Indexes
            builder.HasIndex(p => p.Name);
            builder.HasIndex(p => p.CategoryId);

            builder.MapAuditColumns();
        }
    }
}
