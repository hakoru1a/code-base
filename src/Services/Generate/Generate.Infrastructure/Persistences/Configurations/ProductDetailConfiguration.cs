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

            // Primary key - ProductId is the primary key (not a separate Id)
            builder.HasKey(pd => pd.ProductId);

            // Properties
            builder.Property(pd => pd.ProductId)
                .IsRequired();

            builder.Property(pd => pd.Description)
                .HasColumnType("TEXT");

            // Relationships
            // One-to-One with Product
            builder.HasOne(pd => pd.Product)
                .WithOne(p => p.ProductDetail)
                .HasForeignKey<ProductDetail>(pd => pd.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.MapAuditColumns();
        }
    }
}
