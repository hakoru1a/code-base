
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Contracts.Common.Events;
using Contracts.Domain.Event.Product;

namespace Base.Domain.Entities
{
    public class Product : AuditableEventEntity<long>
    {
        [Required]
        [Column(TypeName = "nvarchar(255)")]
        public string Name { get; set; } = string.Empty;
        [Column(TypeName = "nvarchar(255)")]
        [Required]
        public string Description { get; set; } = string.Empty;
        [Column(TypeName = "decimal(18, 2)")]
        [Required]
        public decimal Price { get; set; }
        [Required]
        [Range(0, int.MaxValue)]
        public int Stock { get; set; }
        [Column(TypeName = "nvarchar(255)")]
        [Required]
        public string SKU { get; set; } = string.Empty;
        protected Product() { }
        private Product(string name, string description, decimal price, int stock, string sku)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Product name is required.", nameof(name));
            if (price <= 0)
                throw new ArgumentException("Price must be greater than zero.", nameof(price));
            if (string.IsNullOrWhiteSpace(sku))
                throw new ArgumentException("SKU is required.", nameof(sku));

            Name = name;
            Description = description;
            Price = price;
            Stock = stock;
            SKU = sku;
        }

        public static Product Create(string name, string description, decimal price, int stock, string sku)
        {
            var product = new Product(name, description, price, stock, sku);

            product.AddDomainEvent(new ProductCreatedEvent(product.Id, product.Name, product.SKU, product.Price, product.Stock));

            return product;
        }

        // Business Methods
        public void UpdateProduct(string name, string description, decimal price, string sku)
        {
            ValidateProductUpdate(name, price, sku);

            var oldName = Name;
            var oldDescription = Description;
            var oldPrice = Price;
            var oldSku = SKU;

            Name = name;
            Description = description;
            Price = price;
            SKU = sku;
            LastModifiedDate = DateTimeOffset.UtcNow;

            AddDomainEvent(new ProductUpdatedEvent(Id, Name, Description, Price, Stock, SKU, LastModifiedDate.Value));
        }

        public void UpdateStock(int newStock)
        {
            ValidateStockUpdate(newStock);

            var oldStock = Stock;
            Stock = newStock;
            LastModifiedDate = DateTimeOffset.UtcNow;

            AddDomainEvent(new StockChangedEvent(Id, Name, SKU, oldStock, Stock, Stock - oldStock, LastModifiedDate.Value));
        }

        public void IncreaseStock(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

            UpdateStock(Stock + quantity);
        }

        public void DecreaseStock(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

            if (Stock < quantity)
                throw new InvalidOperationException($"Insufficient stock. Current stock: {Stock}, requested: {quantity}");

            UpdateStock(Stock - quantity);
        }

        public void DeleteProduct()
        {
            AddDomainEvent(new ProductDeletedEvent(Id, Name, SKU, DateTimeOffset.UtcNow));
        }

        public bool IsInStock()
        {
            return Stock > 0;
        }

        public bool IsLowStock(int threshold = 10)
        {
            return Stock <= threshold && Stock > 0;
        }

        public bool IsOutOfStock()
        {
            return Stock == 0;
        }

        // Validation Methods
        private void ValidateProductUpdate(string name, decimal price, string sku)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Product name is required.", nameof(name));
            if (price <= 0)
                throw new ArgumentException("Price must be greater than zero.", nameof(price));
            if (string.IsNullOrWhiteSpace(sku))
                throw new ArgumentException("SKU is required.", nameof(sku));
        }

        private void ValidateStockUpdate(int newStock)
        {
            if (newStock < 0)
                throw new ArgumentException("Stock cannot be negative.", nameof(newStock));
        }

        // Business Rules
        public decimal CalculateTotalValue()
        {
            return Price * Stock;
        }

        public bool CanFulfillOrder(int requestedQuantity)
        {
            return Stock >= requestedQuantity;
        }

        public int GetAvailableQuantity()
        {
            return Math.Max(0, Stock);
        }
    }
}
