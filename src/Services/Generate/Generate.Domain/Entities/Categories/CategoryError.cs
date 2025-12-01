using Contracts.Exceptions;

namespace Generate.Domain.Entities.Categories;

public static class CategoryError
{
    // Category name validation errors
    public static BusinessException NameCannotBeEmpty()
        => new("Category name cannot be empty");

    public static BusinessException NameTooLong(int maxLength = 100)
        => new($"Category name cannot exceed {maxLength} characters");

    public static BusinessException NameHasInvalidFormat()
        => new("Category name cannot have leading or trailing spaces");

    // Product management errors
    public static BusinessException ProductCannotBeNull()
        => new("Product cannot be null");

    public static BusinessException ProductAlreadyExists()
        => new("Product already exists in this category");

    public static BusinessException ProductNotFoundInCategory()
        => new("Product not found in this category");

    // Business rule errors
    public static BusinessException CannotDeleteCategoryWithProducts()
        => new("Cannot delete category that contains products");

    public static BusinessException CategoryIsEmpty()
        => new("Category must contain at least one product");

    public static BusinessException MaxProductsLimitExceeded(int maxProducts)
        => new($"Category cannot contain more than {maxProducts} products");

    // Hierarchy errors (for future extensions)
    public static BusinessException ParentCategoryNotFound(long parentId)
        => new($"Parent category with ID {parentId} not found");

    public static BusinessException CircularReferenceDetected()
        => new("Category cannot be its own parent or create circular reference");
}

