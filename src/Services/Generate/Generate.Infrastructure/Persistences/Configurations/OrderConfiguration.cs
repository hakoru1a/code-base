using Generate.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Generate.Infrastructure.Persistences.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            // Table name
            builder.ToTable("ORDERS");

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
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade); // Delete all order items when order is deleted

            // Indexes
            builder.HasIndex(o => o.CustomerName);

            builder.MapAuditColumns();
        }
    }
}
