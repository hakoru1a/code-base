using Contracts.Domain;
using Generate.Domain.Entities.Categories;
using Generate.Domain.Entities.Orders.ValueObject;
using Generate.Domain.Entities.Products.ValueObject;
using Generate.Domain.Entities.Products.Rules;
using Generate.Domain.Entities.Products.Specifications;
using Contracts.Exceptions;

namespace Generate.Domain.Entities.Products;

public class Product : EntityAuditBase<long>
{
    public string Name { get; private set; } = string.Empty;
    public virtual Category? Category { get; private set; }
    public virtual ProductDetail? ProductDetail { get; private set; }
    private readonly List<OrderItem> _orderItems = new();
    public virtual IReadOnlyList<OrderItem> OrderItems => _orderItems.AsReadOnly();

    private Product() { }

    // Domain constructor
    public Product(string name, Category? category = null, ProductDetail? productDetail = null)
    {
        ProductValidationRules.ProductName.ValidateProductName(name);
        Name = name;
        Category = category;
        ProductDetail = productDetail;
    }

    // Factory method
    public static Product Create(string name, Category? category = null, ProductDetail? productDetail = null)
    {
        return new Product(name, category, productDetail);
    }

    // Business methods
    public void UpdateName(string name)
    {
        ProductValidationRules.ProductName.ValidateProductName(name);
        Name = name;
    }

    public void AssignToCategory(Category category)
    {
        ProductBusinessRules.CategoryManagement.AssignToCategory(this, category);
        Category = category;
    }

    public void RemoveFromCategory()
    {
        ProductBusinessRules.CategoryManagement.RemoveFromCategory(this);
        Category = null;
    }

    public void UpdateProductDetail(ProductDetail productDetail)
    {
        ProductBusinessRules.ProductDetailManagement.UpdateProductDetail(this, productDetail);
        ProductDetail = productDetail;
    }

    public void AddOrderItem(OrderItem orderItem)
    {
        ProductBusinessRules.OrderItemManagement.AddOrderItem(_orderItems, orderItem);
    }

    public void RemoveOrderItem(OrderItem orderItem)
    {
        ProductBusinessRules.OrderItemManagement.RemoveOrderItem(_orderItems, orderItem);
    }


    // Business rules and queries - sử dụng Business Rules và Specifications
    public bool CanBeDeleted()
    {
        return ProductBusinessRules.Lifecycle.CanBeDeleted(_orderItems);
    }

    public bool IsInCategory()
    {
        return ProductBusinessRules.Lifecycle.IsInCategory(Category);
    }

    public int GetOrderItemsCount()
    {
        return ProductBusinessRules.Analytics.CalculateOrderItemsCount(_orderItems);
    }

    public decimal GetTotalOrderedQuantity()
    {
        return ProductBusinessRules.Analytics.CalculateTotalOrderedQuantity(_orderItems);
    }

    public bool HasActiveOrders()
    {
        return ProductBusinessRules.Lifecycle.HasActiveOrders(_orderItems);
    }

    // Sử dụng Specifications cho business queries phức tạp
    public bool SatisfiesSpecification(ProductSpecifications.IProductSpecification specification)
    {
        return specification.IsSatisfiedBy(this);
    }
}
