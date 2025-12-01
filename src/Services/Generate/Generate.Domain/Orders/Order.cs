using Contracts.Domain;
using Generate.Domain.Products;
using Generate.Domain.Orders.Rules;
using Contracts.Exceptions;
using Contracts.Domain.Interface;

namespace Generate.Domain.Orders;

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
        CustomerName = customerName;
    }

    public void AddOrderItem(Product product, int quantity)
    {
        CheckRule(new OrderProductRequiredRule(product));
        CheckRule(new OrderQuantityValidRule(quantity));
        CheckRule(new OrderProductNotExistsRule(_orderItems, product));
        CheckRule(new OrderMaxItemsLimitRule(_orderItems));

        var orderItem = new OrderItem(this, product, quantity);
        _orderItems.Add(orderItem);
    }

    public void RemoveOrderItem(Product product)
    {
        CheckRule(new OrderProductRequiredRule(product));
        CheckRule(new OrderProductExistsRule(_orderItems, product));

        var orderItem = _orderItems.FirstOrDefault(oi => ReferenceEquals(oi.Product, product));
        if (orderItem != null)
        {
            _orderItems.Remove(orderItem);
        }
    }

    public void UpdateOrderItemQuantity(Product product, int newQuantity)
    {
        CheckRule(new OrderProductRequiredRule(product));
        CheckRule(new OrderQuantityValidRule(newQuantity));
        CheckRule(new OrderProductExistsRule(_orderItems, product));

        var orderItem = _orderItems.FirstOrDefault(oi => ReferenceEquals(oi.Product, product));
        if (orderItem != null)
        {
            orderItem.UpdateQuantity(newQuantity);
        }
    }

    public void ClearOrderItems()
    {
        _orderItems.Clear();
    }


    // Business rules and queries - sử dụng Specifications và Business Rules
    public bool CanBeDeleted()
    {
        var rule = new OrderCanBeDeletedRule(_orderItems);
        return !rule.IsBroken();
    }

    public bool HasOrderItems()
    {
        return _orderItems.Any();
    }

    public int GetTotalItemsCount()
    {
        return _orderItems.Count;
    }

    public int GetUniqueProductsCount()
    {
        return _orderItems.DistinctBy(oi => oi.Product.Id).Count();
    }

    public bool ContainsProduct(Product product)
    {
        return _orderItems.Any(oi => oi.Product.Id == product.Id);
    }

    public OrderItem? GetOrderItem(Product product)
    {
        return _orderItems.FirstOrDefault(oi => oi.Product.Id == product.Id);
    }

    public IReadOnlyList<OrderItem> GetOrderItemsForProduct(Product product)
    {
        return _orderItems.Where(oi => oi.Product.Id == product.Id).ToList();
    }

    public bool IsLargeOrder(int threshold = 50)
    {
        CheckRule(new OrderThresholdValidRule(threshold));
        return GetTotalItemsCount() > threshold;
    }

    public decimal GetTotalOrderValue()
    {
        return _orderItems.Sum(oi => oi.Quantity * oi.Product.ProductDetail?.Price ?? 0);
    }
}

