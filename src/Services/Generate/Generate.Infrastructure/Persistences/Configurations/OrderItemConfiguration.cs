using Generate.Domain.Entities.Orders.ValueObject;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Generate.Infrastructure.Persistences.Configurations
{
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        private readonly string TableName = "ORDER_ITEM";
        private readonly string OrderIdColumn = "ORDER_ID";
        private readonly string ProductIdColumn = "PRODUCT_ID";
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            // Table name
            builder.ToTable(TableName);

            // Configure shadow properties for foreign keys (not in domain model)
            builder.Property<long>(OrderIdColumn).IsRequired();
            builder.Property<long>(ProductIdColumn).IsRequired();

            // Composite primary key using shadow properties
            builder.HasKey(OrderIdColumn, ProductIdColumn);

            // Domain properties
            builder.Property(oi => oi.Quantity)
                .IsRequired();

            // Relationships using shadow properties as foreign keys
            // Many-to-One with Order
            builder.HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(OrderIdColumn)
                .OnDelete(DeleteBehavior.Cascade);

            // Many-to-One with Product
            builder.HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(ProductIdColumn)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deletion if product is in orders

            // Indexes on shadow properties
            builder.HasIndex(OrderIdColumn);
            builder.HasIndex(ProductIdColumn);

            builder.MapAuditColumns();
        }
    }
}
