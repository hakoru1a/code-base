using Contracts.Domain;
using Generate.Domain.Entities.Orders.ValueObject;
using Generate.Domain.Entities.Products;
using Contracts.Exceptions;

namespace Generate.Domain.Entities.Orders;

public class Order : EntityAuditBase<long>
{
    public string CustomerName { get; private set; } = string.Empty;
    
    private readonly List<OrderItem> _orderItems = new();
    public virtual IReadOnlyList<OrderItem> OrderItems => _orderItems.AsReadOnly();

    // EF Core constructor
    private Order() { }

    // Domain constructor
    public Order(string customerName)
    {
        ValidateCustomerName(customerName);
        CustomerName = customerName;
    }

    // Factory method
    public static Order Create(string customerName)
    {
        return new Order(customerName);
    }

    // Business methods
    public void UpdateCustomerName(string customerName)
    {
        ValidateCustomerName(customerName);
        CustomerName = customerName;
    }

    public void AddOrderItem(Product product, int quantity)
    {
        if (product == null)
            throw ProductError.CategoryCannotBeNull();

        // Check if product already exists in order - compare navigation properties
        var existingItem = _orderItems.FirstOrDefault(oi => ReferenceEquals(oi.Product, product));
        if (existingItem != null)
        {
            existingItem.IncreaseQuantity(quantity);
        }
        else
        {
            var orderItem = OrderItem.Create(this, product, quantity);
            _orderItems.Add(orderItem);
        }
    }

    public void RemoveOrderItem(Product product)
    {
        if (product == null)
            throw ProductError.CategoryCannotBeNull();
            
        var orderItem = _orderItems.FirstOrDefault(oi => ReferenceEquals(oi.Product, product));
        if (orderItem == null)
            throw new BusinessException($"Product not found in this order");

        _orderItems.Remove(orderItem);
    }

    public void UpdateOrderItemQuantity(Product product, int newQuantity)
    {
        if (product == null)
            throw ProductError.CategoryCannotBeNull();
            
        var orderItem = _orderItems.FirstOrDefault(oi => ReferenceEquals(oi.Product, product));
        if (orderItem == null)
            throw new BusinessException($"Product not found in this order");

        orderItem.UpdateQuantity(newQuantity);
    }

    public void ClearOrderItems()
    {
        _orderItems.Clear();
    }

    // Domain validation
    private static void ValidateCustomerName(string customerName)
    {
        if (string.IsNullOrWhiteSpace(customerName))
            throw new BusinessException("Customer name cannot be empty");

        if (customerName.Length > 100)
            throw new BusinessException("Customer name cannot exceed 100 characters");

        if (customerName.Trim() != customerName)
            throw new BusinessException("Customer name cannot have leading or trailing spaces");
    }

    // Business rules and queries
    public bool CanBeDeleted()
    {
        return !_orderItems.Any();
    }

    public bool HasOrderItems()
    {
        return _orderItems.Any();
    }

    public int GetTotalItemsCount()
    {
        return _orderItems.Sum(oi => oi.Quantity);
    }

    public int GetUniqueProductsCount()
    {
        return _orderItems.Count;
    }

    public bool ContainsProduct(Product product)
    {
        if (product == null) return false;
        return _orderItems.Any(oi => ReferenceEquals(oi.Product, product));
    }

    public OrderItem? GetOrderItem(Product product)
    {
        if (product == null) return null;
        return _orderItems.FirstOrDefault(oi => ReferenceEquals(oi.Product, product));
    }

    public IReadOnlyList<OrderItem> GetOrderItemsForProduct(Product product)
    {
        if (product == null) return new List<OrderItem>().AsReadOnly();
        return _orderItems.Where(oi => ReferenceEquals(oi.Product, product)).ToList().AsReadOnly();
    }

    public bool IsLargeOrder(int threshold = 50)
    {
        return GetTotalItemsCount() >= threshold;
    }

    public decimal GetTotalOrderValue()
    {
        // This would require product prices - for now return total quantity
        return GetTotalItemsCount();
    }
}
