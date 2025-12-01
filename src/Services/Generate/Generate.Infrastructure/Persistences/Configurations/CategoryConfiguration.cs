
using Generate.Domain.Categories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Generate.Infrastructure.Persistences.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        private readonly string TableName = "CATEGORY";
        private readonly string CategoryIdColumn = "CATEGORY_ID";
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            // Table name
            builder.ToTable(TableName);

            // Primary key
            builder.HasKey(c => c.Id);

            // Properties
            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            // Relationships
            builder.HasMany(c => c.Products)
                .WithOne(p => p.Category)
                .HasForeignKey(CategoryIdColumn)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            builder.HasIndex(c => c.Name);

            builder.MapAuditColumns();
        }
    }
}
