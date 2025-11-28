using Contracts.Exceptions;

namespace Generate.Domain.Entities.Products;

public static class ProductError
{
    // Product name validation errors
    public static BusinessException NameCannotBeEmpty()
        => new("Product name cannot be empty");

    public static BusinessException NameTooLong(int maxLength = 100)
        => new($"Product name cannot exceed {maxLength} characters");

    public static BusinessException NameHasInvalidFormat()
        => new("Product name cannot have leading or trailing spaces");

    // Category related errors
    public static BusinessException CategoryCannotBeNull()
        => new("Category cannot be null when assigning to product");

    public static BusinessException CategoryNotFound(long categoryId)
        => new($"Category with ID {categoryId} not found");

    // Product detail errors
    public static BusinessException ProductDetailAlreadyExists()
        => new("Product detail already exists for this product");

    public static BusinessException ProductDetailNotFound()
        => new("Product detail not found for this product");

    // Order item errors
    public static BusinessException OrderItemCannotBeNull()
        => new("OrderItem cannot be null");

    public static BusinessException OrderItemAlreadyExists()
        => new("OrderItem already exists");

    public static BusinessException InvalidQuantity()
        => new("Order item quantity must be greater than zero");

    // Business rule errors
    public static BusinessException CannotDeleteProductWithOrders()
        => new("Cannot delete product that has existing orders");

    public static BusinessException ProductNotInCategory()
        => new("Product is not assigned to any category");

    // Stock/Inventory errors (for future use)
    public static BusinessException InsufficientStock(int requested, int available)
        => new($"Insufficient stock. Requested: {requested}, Available: {available}");

    public static BusinessException ProductDiscontinued()
        => new("Product has been discontinued and cannot be ordered");
}
