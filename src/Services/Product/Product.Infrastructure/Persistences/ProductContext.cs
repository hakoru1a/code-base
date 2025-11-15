using Contracts.Common.Events;
using Contracts.Common.Interface;
using Contracts.Domain.Interface;
using Product.Domain.Entities;
using Infrastructure.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Interfaces.Event;
using System.Reflection;
using ProductEntity = Product.Domain.Entities.Product;

namespace Product.Infrastructure.Persistences
{
    public class ProductContext : DbContext
    {
        private IMediator _mediator;

        private List<BaseEvent>? _events;

        public ProductContext(DbContextOptions<ProductContext> options, IMediator mediator) : base(options)
        {
            _mediator = mediator;
        }

        // DbSets
        public DbSet<ProductEntity> Products { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<AttributeDef> AttributeDefs { get; set; }
        public DbSet<ProductVariantAttribute> ProductVariantAttributes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Ignore<BaseEvent>();
            
            // Product configuration
            modelBuilder.Entity<ProductEntity>(entity =>
            {
                entity.ToTable("PRODUCT");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Description).HasMaxLength(2000);
                entity.Property(e => e.Slug).HasMaxLength(255);
                entity.Property(e => e.SKU).HasMaxLength(100);
                entity.Property(e => e.Price).HasPrecision(18, 2);
                entity.Property(e => e.ComparePrice).HasPrecision(18, 2);
                entity.Property(e => e.Status).HasConversion<int>();

                // Indexes for better performance
                entity.HasIndex(e => e.SKU).IsUnique().HasDatabaseName("IX_Product_SKU");
                entity.HasIndex(e => e.Slug).IsUnique().HasDatabaseName("IX_Product_Slug");
                entity.HasIndex(e => e.Name).HasDatabaseName("IX_Product_Name");
                entity.HasIndex(e => e.CategoryId).HasDatabaseName("IX_Product_CategoryId");
                entity.HasIndex(e => e.Status).HasDatabaseName("IX_Product_Status");
                entity.HasIndex(e => e.CreatedDate).HasDatabaseName("IX_Product_CreatedDate");
                entity.HasIndex(e => new { e.Status, e.CategoryId }).HasDatabaseName("IX_Product_Status_Category");
                entity.HasIndex(e => new { e.Name, e.Status }).HasDatabaseName("IX_Product_Name_Status");

                entity.HasMany(e => e.Variants)
                    .WithOne(e => e.Product)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ProductVariant configuration
            modelBuilder.Entity<ProductVariant>(entity =>
            {
                entity.ToTable("PRODUCT_VARIANT");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.SKU).HasMaxLength(100);
                entity.Property(e => e.Price).HasPrecision(18, 2);
                entity.Property(e => e.ComparePrice).HasPrecision(18, 2);
                entity.Property(e => e.Status).HasConversion<int>();

                // Indexes for better performance
                entity.HasIndex(e => e.SKU).IsUnique().HasDatabaseName("IX_ProductVariant_SKU");
                entity.HasIndex(e => e.ProductId).HasDatabaseName("IX_ProductVariant_ProductId");
                entity.HasIndex(e => e.Status).HasDatabaseName("IX_ProductVariant_Status");
                entity.HasIndex(e => new { e.ProductId, e.Status }).HasDatabaseName("IX_ProductVariant_Product_Status");
                entity.HasIndex(e => e.InventoryQuantity).HasDatabaseName("IX_ProductVariant_Inventory");

                entity.HasMany(e => e.Attributes)
                    .WithOne(e => e.ProductVariant)
                    .HasForeignKey(e => e.ProductVariantId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // AttributeDef configuration
            modelBuilder.Entity<AttributeDef>(entity =>
            {
                entity.ToTable("ATTRIBUTE_DEF");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Type).HasConversion<int>();
                entity.Property(e => e.Options).HasMaxLength(2000);

                // Indexes for better performance
                entity.HasIndex(e => e.Name).IsUnique().HasDatabaseName("IX_AttributeDef_Name");
                entity.HasIndex(e => e.Type).HasDatabaseName("IX_AttributeDef_Type");
                entity.HasIndex(e => e.IsActive).HasDatabaseName("IX_AttributeDef_IsActive");
                entity.HasIndex(e => e.DisplayOrder).HasDatabaseName("IX_AttributeDef_DisplayOrder");

                entity.HasMany(e => e.ProductVariantAttributes)
                    .WithOne(e => e.AttributeDef)
                    .HasForeignKey(e => e.AttributeDefId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ProductVariantAttribute configuration
            modelBuilder.Entity<ProductVariantAttribute>(entity =>
            {
                entity.ToTable("PRODUCT_VARIANT_ATTRIBUTE");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Value).IsRequired().HasMaxLength(500);

                // Indexes for better performance
                entity.HasIndex(e => e.ProductVariantId).HasDatabaseName("IX_ProductVariantAttribute_VariantId");
                entity.HasIndex(e => e.AttributeDefId).HasDatabaseName("IX_ProductVariantAttribute_AttributeDefId");
                entity.HasIndex(e => e.Value).HasDatabaseName("IX_ProductVariantAttribute_Value");

                // Composite unique constraint
                entity.HasIndex(e => new { e.ProductVariantId, e.AttributeDefId })
                    .IsUnique()
                    .HasDatabaseName("IX_ProductVariantAttribute_Variant_Attribute");
            });

            base.OnModelCreating(modelBuilder);
        }

        private void SaveEventBeforeSaveChanges()
        {
            var domainEntities = ChangeTracker.Entries<IEventEntity>()
                .Select(x => x.Entity)
                .Where(x => x.DomainEvents.Any())
                .ToList();

            var domainEvents = domainEntities
                .SelectMany(x => x.DomainEvents)
                .ToList();

            domainEntities.ForEach(entity => entity.ClearDomainEvents());

            _events = domainEvents;
        }

        public async override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            SaveEventBeforeSaveChanges();

            var modified = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Modified ||
                           e.State == EntityState.Added ||
                           e.State == EntityState.Deleted);

            foreach (var item in modified)
            {
                switch (item.State)
                {
                    case EntityState.Added:
                        if (item.Entity is IDateTracking addedEntity)
                        {
                            addedEntity.CreatedDate = DateTime.UtcNow;
                            item.State = EntityState.Added;
                        }
                        break;

                    case EntityState.Modified:
                        Entry(item.Entity).Property("Id").IsModified = false;
                        if (item.Entity is IDateTracking modifiedEntity)
                        {
                            modifiedEntity.LastModifiedDate = DateTime.UtcNow;
                            item.State = EntityState.Modified;
                        }
                        break;
                }
            }
            var result = await base.SaveChangesAsync(cancellationToken);
            if (_events?.Any() == true)
            {
                await _mediator.DispatchDomainEventAsync(_events);
            }
            return result;
        }
    }
}