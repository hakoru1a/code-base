using Generate.Domain.Entities.Products;

namespace Generate.Domain.Entities.Categories.Rules;

/// <summary>
/// Chứa các business validation rules cho Category entity
/// Tách riêng validation logic ra khỏi entity để dễ maintain và test
/// </summary>
public static class CategoryValidationRules
{
    public static class CategoryName
    {
        public static void ValidateCategoryName(string categoryName)
        {
            ValidateNotEmpty(categoryName);
            ValidateLength(categoryName);
            ValidateFormat(categoryName);
        }

        private static void ValidateNotEmpty(string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
                throw CategoryError.NameCannotBeEmpty();
        }

        private static void ValidateLength(string categoryName, int maxLength = 100)
        {
            if (categoryName.Length > maxLength)
                throw CategoryError.NameTooLong(maxLength);
        }

        private static void ValidateFormat(string categoryName)
        {
            if (categoryName.Trim() != categoryName)
                throw CategoryError.NameHasInvalidFormat();
        }
    }

    public static class ProductManagement
    {
        public static void ValidateProduct(Product? product)
        {
            if (product == null)
                throw CategoryError.ProductCannotBeNull();
        }

        public static void ValidateProductNotExists(List<Product> products, Product product)
        {
            var exists = products.Any(p => p.Id == product.Id);
            if (exists)
                throw CategoryError.ProductAlreadyExists();
        }

        public static void ValidateProductExists(List<Product> products, Product product)
        {
            var exists = products.Any(p => p.Id == product.Id);
            if (!exists)
                throw CategoryError.ProductNotFoundInCategory();
        }
    }

    public static class CategoryConstraints
    {
        public static void ValidateCanBeDeleted(List<Product> products)
        {
            if (products.Any())
                throw CategoryError.CannotDeleteCategoryWithProducts();
        }

        public static void ValidateNotEmpty(List<Product> products)
        {
            if (!products.Any())
                throw CategoryError.CategoryIsEmpty();
        }

        public static void ValidateMaxProductsLimit(List<Product> products, int maxProducts = 1000)
        {
            if (products.Count >= maxProducts)
                throw CategoryError.MaxProductsLimitExceeded(maxProducts);
        }
    }
}
