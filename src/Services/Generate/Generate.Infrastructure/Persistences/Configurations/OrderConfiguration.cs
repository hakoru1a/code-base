using Generate.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Generate.Infrastructure.Persistences.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        private readonly string TableName = "ORDER";
        private readonly string OrderIdColumn = "ORDER_ID";
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            // Table name
            builder.ToTable(TableName); // Match SQL schema table name

            // Primary key
            builder.HasKey(o => o.Id);

            // Properties
            builder.Property(o => o.CustomerName)
                .IsRequired()
                .HasMaxLength(100);

            // Relationships
            // One-to-Many with OrderItems
            builder.HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(OrderIdColumn)
                .OnDelete(DeleteBehavior.Cascade); // Delete all order items when order is deleted

            // Indexes
            builder.HasIndex(o => o.CustomerName);

            builder.MapAuditColumns();
        }
    }
}
