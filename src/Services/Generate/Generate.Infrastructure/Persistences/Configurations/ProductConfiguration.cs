using Generate.Domain.Entities.Products;
using Generate.Domain.Entities.Products.ValueObject;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Generate.Infrastructure.Persistences.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        private readonly string TableName = "PRODUCT";
        private readonly string ProductIdColumn = "PRODUCT_ID";
        private readonly string CategoryIdColumn = "CATEGORY_ID";
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            // Table name
            builder.ToTable(TableName);

            // Primary key
            builder.HasKey(p => p.Id);

            // Properties
            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(100);

            // Configure shadow property for CategoryId (not in domain model)
            builder.Property<long?>(CategoryIdColumn).IsRequired(false);

            // Relationships
            // One-to-Many with Category using shadow property
            builder.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(CategoryIdColumn)
                .OnDelete(DeleteBehavior.SetNull);

            // One-to-One with ProductDetail
            builder.HasOne(p => p.ProductDetail)
                .WithOne(pd => pd.Product)
                .HasForeignKey<ProductDetail>(ProductIdColumn)
                .OnDelete(DeleteBehavior.Cascade);

            // One-to-Many with OrderItems
            builder.HasMany(p => p.OrderItems)
                .WithOne(oi => oi.Product)
                .HasForeignKey(ProductIdColumn)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deletion if product is in orders

            // Indexes
            builder.HasIndex(p => p.Name);
            builder.HasIndex(CategoryIdColumn);

            builder.MapAuditColumns();
        }
    }
}
