using Contracts.Domain;
using Generate.Domain.Entities.Categories;
using Generate.Domain.Entities.Orders.ValueObject;
using Generate.Domain.Entities.Products.ValueObject;
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
        ValidateName(name);
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
        ValidateName(name);
        Name = name;
    }

    public void AssignToCategory(Category category)
    {
        if (category == null)
            throw ProductError.CategoryCannotBeNull();

        Category = category;
    }

    public void RemoveFromCategory()
    {
        Category = null;
    }

    public void UpdateProductDetail(ProductDetail productDetail)
    {
        ProductDetail = productDetail;
    }

    public void AddOrderItem(OrderItem orderItem)
    {
        if (orderItem == null)
            throw ProductError.OrderItemCannotBeNull();

        if (_orderItems.Any(oi => ReferenceEquals(oi.Order, orderItem.Order) && ReferenceEquals(oi.Product, orderItem.Product)))
            throw ProductError.OrderItemAlreadyExists();

        _orderItems.Add(orderItem);
    }

    public void RemoveOrderItem(OrderItem orderItem)
    {
        if (orderItem == null)
            throw ProductError.OrderItemCannotBeNull();

        _orderItems.Remove(orderItem);
    }

    // Domain validation
    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw ProductError.NameCannotBeEmpty();

        if (name.Length > 100) // Match SQL schema VARCHAR(100)
            throw ProductError.NameTooLong(100);

        if (name.Trim() != name)
            throw ProductError.NameHasInvalidFormat();
    }

    // Domain business rules
    public bool CanBeDeleted()
    {
        return !_orderItems.Any();
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
}
