using Generate.Domain.Entities.Orders;
using Generate.Domain.Entities.Orders.Rules;
using Generate.Domain.Entities.Orders.Specifications;
using Generate.Domain.Entities.Products;

namespace Generate.Domain.Services;

/// <summary>
/// Domain Service cho Order entity
/// Chứa business logic phức tạp không thuộc về một entity cụ thể nào
/// hoặc logic cần coordinate giữa nhiều entities/aggregates
/// </summary>
public class OrderDomainService
{
    /// <summary>
    /// Tạo order với validation và business rules phức tạp
    /// </summary>
    public Order CreateOrderWithValidation(string customerName, List<(Product product, int quantity)> items)
    {
        // Tạo order cơ bản
        var order = Order.Create(customerName);
        
        // Validate business rules trước khi thêm items
        if (!items.Any())
            throw OrderError.OrderIsEmpty();

        // Thêm từng item với validation
        foreach (var (product, quantity) in items)
        {
            order.AddOrderItem(product, quantity);
        }

        return order;
    }

    /// <summary>
    /// Transfer items từ order này sang order khác
    /// Logic phức tạp cần coordinate 2 orders
    /// </summary>
    public void TransferItems(Order sourceOrder, Order targetOrder, List<Product> productsToTransfer)
    {
        if (sourceOrder == null) throw new ArgumentNullException(nameof(sourceOrder));
        if (targetOrder == null) throw new ArgumentNullException(nameof(targetOrder));
        if (!productsToTransfer.Any()) return;

        foreach (var product in productsToTransfer)
        {
            var sourceItem = sourceOrder.GetOrderItem(product);
            if (sourceItem == null) continue;

            // Transfer quantity
            targetOrder.AddOrderItem(product, sourceItem.Quantity);
            sourceOrder.RemoveOrderItem(product);
        }
    }

    /// <summary>
    /// Merge hai orders thành một
    /// Business logic phức tạp coordinate 2 aggregates
    /// </summary>
    public Order MergeOrders(Order primaryOrder, Order secondaryOrder)
    {
        if (primaryOrder == null) throw new ArgumentNullException(nameof(primaryOrder));
        if (secondaryOrder == null) throw new ArgumentNullException(nameof(secondaryOrder));

        // Transfer tất cả items từ secondary sang primary
        var itemsToTransfer = secondaryOrder.OrderItems
            .Select(oi => oi.Product)
            .ToList();

        TransferItems(secondaryOrder, primaryOrder, itemsToTransfer);

        return primaryOrder;
    }

    /// <summary>
    /// Validate order theo multiple business rules
    /// </summary>
    public bool ValidateOrder(Order order, out List<string> validationErrors)
    {
        validationErrors = new List<string>();

        try
        {
            // Check basic rules
            if (!order.HasOrderItems())
                validationErrors.Add("Order must have at least one item");

            // Check specifications
            var canBeDeletedSpec = new OrderSpecifications.CanBeDeletedSpecification();
            var hasItemsSpec = new OrderSpecifications.HasItemsSpecification();
            
            // Business rule: Order với items không thể bị xóa ngay
            if (hasItemsSpec.IsSatisfiedBy(order) && canBeDeletedSpec.IsSatisfiedBy(order))
                validationErrors.Add("Inconsistent order state");

            return !validationErrors.Any();
        }
        catch (Exception ex)
        {
            validationErrors.Add($"Validation error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Tính toán thống kê order theo nhiều tiêu chí
    /// </summary>
    public OrderStatistics CalculateOrderStatistics(Order order)
    {
        var largeOrderSpec = new OrderSpecifications.IsLargeOrderSpecification();
        var hasItemsSpec = new OrderSpecifications.HasItemsSpecification();

        return new OrderStatistics
        {
            TotalItems = order.GetTotalItemsCount(),
            UniqueProducts = order.GetUniqueProductsCount(),
            TotalValue = order.GetTotalOrderValue(),
            IsLargeOrder = largeOrderSpec.IsSatisfiedBy(order),
            HasItems = hasItemsSpec.IsSatisfiedBy(order),
            CanBeDeleted = order.CanBeDeleted()
        };
    }
}

/// <summary>
/// Value object cho order statistics
/// </summary>
public record OrderStatistics
{
    public int TotalItems { get; init; }
    public int UniqueProducts { get; init; }
    public decimal TotalValue { get; init; }
    public bool IsLargeOrder { get; init; }
    public bool HasItems { get; init; }
    public bool CanBeDeleted { get; init; }
}
