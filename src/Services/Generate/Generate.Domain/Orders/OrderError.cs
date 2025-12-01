using Contracts.Exceptions;

namespace Generate.Domain.Orders;

public static class OrderError
{
    // Customer validation errors
    public static BusinessException CustomerNameCannotBeEmpty()
        => new("Customer name cannot be empty");

    public static BusinessException CustomerNameTooLong(int maxLength = 100)
        => new($"Customer name cannot exceed {maxLength} characters");

    public static BusinessException CustomerNameHasInvalidFormat()
        => new("Customer name cannot have leading or trailing spaces");

    // Order item errors
    public static BusinessException ProductCannotBeNull()
        => new("Product cannot be null");

    public static BusinessException ProductNotFoundInOrder()
        => new("Product not found in this order");

    public static BusinessException OrderItemAlreadyExists()
        => new("Product already exists in this order");

    public static BusinessException InvalidQuantity()
        => new("Quantity must be greater than zero");

    // Business rule errors
    public static BusinessException CannotDeleteOrderWithItems()
        => new("Cannot delete order that contains items");

    public static BusinessException OrderIsEmpty()
        => new("Order must contain at least one item");

    public static BusinessException MaxItemsLimitExceeded(int maxItems)
        => new($"Order cannot contain more than {maxItems} items");

    // Threshold errors
    public static BusinessException InvalidThreshold(int threshold)
        => new($"Threshold must be greater than zero, got: {threshold}");
}

