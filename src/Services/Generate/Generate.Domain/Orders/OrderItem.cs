using Contracts.Domain;
using Generate.Domain.Products;

namespace Generate.Domain.Orders;

public class OrderItem : AuditableBase<long>
{
    public int Quantity { get; private set; }

    // Navigation properties
    public virtual Order Order { get; private set; } = null!;
    public virtual Product Product { get; private set; } = null!;

    private OrderItem() { }

    // Domain constructor
    public OrderItem(Order order, Product product, int quantity)
    {
        if (order == null)
            throw new Contracts.Exceptions.BusinessException("Order cannot be null");
        if (product == null)
            throw ProductError.CategoryCannotBeNull();

        ValidateQuantity(quantity);
        Order = order;
        Product = product;
        Quantity = quantity;
        CreatedDate = DateTime.UtcNow;
    }

    // Factory method
    public static OrderItem Create(Order order, Product product, int quantity)
    {
        return new OrderItem(order, product, quantity);
    }

    // Business methods
    public void UpdateQuantity(int quantity)
    {
        ValidateQuantity(quantity);
        Quantity = quantity;
    }

    public void IncreaseQuantity(int additionalQuantity)
    {
        if (additionalQuantity <= 0)
            throw ProductError.InvalidQuantity();

        UpdateQuantity(Quantity + additionalQuantity);
    }

    public void DecreaseQuantity(int reductionQuantity)
    {
        if (reductionQuantity <= 0)
            throw ProductError.InvalidQuantity();

        var newQuantity = Quantity - reductionQuantity;
        if (newQuantity <= 0)
            throw ProductError.InvalidQuantity();

        UpdateQuantity(newQuantity);
    }

    public void AssignToOrder(Order order)
    {
        if (order == null)
            throw new Contracts.Exceptions.BusinessException("Order cannot be null");

        Order = order;
    }

    public void AssignToProduct(Product product)
    {
        if (product == null)
            throw ProductError.CategoryCannotBeNull();

        Product = product;
    }

    // Domain validation
    private static void ValidateQuantity(int quantity)
    {
        if (quantity <= 0)
            throw ProductError.InvalidQuantity();
    }

    // Business queries
    public decimal GetTotalValue()
    {
        // This would require product price - for now return quantity
        return Quantity;
    }

    public bool IsLargeOrder(int threshold = 100)
    {
        return Quantity >= threshold;
    }
}

