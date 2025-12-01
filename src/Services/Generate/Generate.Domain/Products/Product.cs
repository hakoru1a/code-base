using Contracts.Domain;
using Generate.Domain.Categories;
using Generate.Domain.Orders;
using Generate.Domain.Products.Rules;
using Contracts.Exceptions;
using Contracts.Domain.Interface;

namespace Generate.Domain.Products;

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
        Name = name;
    }

    public void AssignToCategory(Category category)
    {
        CheckRule(new ProductCategoryRequiredRule(category));
        Category = category;
    }

    public void RemoveFromCategory()
    {
        Category = null;
    }

    public void UpdateProductDetail(ProductDetail productDetail)
    {
        CheckRule(new ProductDetailNotExistsRule(ProductDetail));
        ProductDetail = productDetail;
    }

    public void AddOrderItem(OrderItem orderItem)
    {
        CheckRule(new ProductOrderItemRequiredRule(orderItem));
        CheckRule(new ProductOrderItemNotExistsRule(_orderItems, orderItem));
        _orderItems.Add(orderItem);
    }

    public void RemoveOrderItem(OrderItem orderItem)
    {
        CheckRule(new ProductOrderItemRequiredRule(orderItem));
        CheckRule(new ProductOrderItemExistsRule(_orderItems, orderItem));
        _orderItems.Remove(orderItem);
    }


    // Business rules and queries - sử dụng Business Rules và Specifications
    public bool CanBeDeleted()
    {
        var rule = new ProductCanBeDeletedRule(_orderItems);
        return !rule.IsBroken();
    }

    public bool IsInCategory()
    {
        return Category != null;
    }

    public int GetOrderItemsCount()
    {
        return _orderItems.Count;
    }

    public decimal GetTotalOrderedQuantity()
    {
        return _orderItems.Sum(oi => oi.Quantity);
    }

    public bool HasActiveOrders()
    {
        return _orderItems.Any();
    }
}

