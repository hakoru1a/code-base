using Contracts.Domain;
using Generate.Domain.Entities.Orders.ValueObject;
using Generate.Domain.Entities.Products;
using Generate.Domain.Entities.Orders.Rules;
using Generate.Domain.Entities.Orders.Specifications;
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
        OrderValidationRules.CustomerName.ValidateCustomerName(customerName);
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
        OrderValidationRules.CustomerName.ValidateCustomerName(customerName);
        CustomerName = customerName;
    }

    public void AddOrderItem(Product product, int quantity)
    {
        OrderBusinessRules.ItemManagement.AddOrderItem(_orderItems, this, product, quantity);
    }

    public void RemoveOrderItem(Product product)
    {
        OrderBusinessRules.ItemManagement.RemoveOrderItem(_orderItems, product);
    }

    public void UpdateOrderItemQuantity(Product product, int newQuantity)
    {
        OrderBusinessRules.ItemManagement.UpdateOrderItemQuantity(_orderItems, product, newQuantity);
    }

    public void ClearOrderItems()
    {
        _orderItems.Clear();
    }


    // Business rules and queries - sử dụng Specifications và Business Rules
    public bool CanBeDeleted()
    {
        return OrderBusinessRules.Lifecycle.CanBeDeleted(_orderItems);
    }

    public bool HasOrderItems()
    {
        return OrderBusinessRules.Lifecycle.HasOrderItems(_orderItems);
    }

    public int GetTotalItemsCount()
    {
        return OrderBusinessRules.Analytics.CalculateTotalItemsCount(_orderItems);
    }

    public int GetUniqueProductsCount()
    {
        return OrderBusinessRules.Analytics.CalculateUniqueProductsCount(_orderItems);
    }

    public bool ContainsProduct(Product product)
    {
        return OrderBusinessRules.Query.ContainsProduct(_orderItems, product);
    }

    public OrderItem? GetOrderItem(Product product)
    {
        return OrderBusinessRules.Query.GetOrderItem(_orderItems, product);
    }

    public IReadOnlyList<OrderItem> GetOrderItemsForProduct(Product product)
    {
        return OrderBusinessRules.Query.GetOrderItemsForProduct(_orderItems, product);
    }

    public bool IsLargeOrder(int threshold = 50)
    {
        return OrderBusinessRules.Analytics.IsLargeOrder(_orderItems, threshold);
    }

    public decimal GetTotalOrderValue()
    {
        return OrderBusinessRules.Analytics.CalculateTotalOrderValue(_orderItems);
    }

    // Sử dụng Specifications cho business queries phức tạp
    public bool SatisfiesSpecification(OrderSpecifications.IOrderSpecification specification)
    {
        return specification.IsSatisfiedBy(this);
    }
}
