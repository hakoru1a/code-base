using Generate.Domain.Entities.Products.ValueObject;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Data;

namespace Generate.Infrastructure.Persistences.Configurations
{
    public class ProductDetailConfiguration : IEntityTypeConfiguration<ProductDetail>
    {
        private readonly string TableName = "PRODUCT_DETAIL";
        private readonly string ProductIdColumn = "PRODUCT_ID";
        private readonly string DescriptionColumn = "DESCRIPTION";
        public void Configure(EntityTypeBuilder<ProductDetail> builder)
        {
            // Table name
            builder.ToTable(TableName);

            // Configure shadow property for ProductId as primary key
            builder.Property<long>(ProductIdColumn)
                .HasColumnName(ProductIdColumn)
                .IsRequired();

            // Primary key is ProductId (shadow property)
            builder.HasKey(ProductIdColumn);

            // Properties
            builder.Property(pd => pd.Description)
                .HasColumnName(DescriptionColumn)
                .HasColumnType(SqlDbType.Text.ToString());

            // Relationships
            // One-to-One with Product using ProductId shadow property
            builder.HasOne(pd => pd.Product)
                .WithOne(p => p.ProductDetail)
                .HasForeignKey<ProductDetail>(ProductIdColumn)
                .OnDelete(DeleteBehavior.Cascade);

            builder.MapAuditColumns();
        }
    }
}
