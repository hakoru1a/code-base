using Generate.Domain.Entities.Products;

namespace Generate.Domain.Entities.Categories.Specifications;

/// <summary>
/// Specification Pattern cho Category entity
/// Tách riêng business queries và conditions ra khỏi entity
/// </summary>
public static class CategorySpecifications
{
    /// <summary>
    /// Base interface cho Category specifications
    /// </summary>
    public interface ICategorySpecification
    {
        bool IsSatisfiedBy(Category category);
    }

    /// <summary>
    /// Specification để check category có thể bị xóa không
    /// </summary>
    public class CanBeDeletedSpecification : ICategorySpecification
    {
        public bool IsSatisfiedBy(Category category)
        {
            return !category.Products.Any();
        }
    }

    /// <summary>
    /// Specification để check category có products không
    /// </summary>
    public class HasProductsSpecification : ICategorySpecification
    {
        public bool IsSatisfiedBy(Category category)
        {
            return category.Products.Any();
        }
    }

    /// <summary>
    /// Specification để check category có phải là large category không
    /// </summary>
    public class IsLargeCategorySpecification : ICategorySpecification
    {
        private readonly int _threshold;

        public IsLargeCategorySpecification(int threshold = 50)
        {
            if (threshold <= 0)
                throw new ArgumentException("Threshold must be greater than zero");

            _threshold = threshold;
        }

        public bool IsSatisfiedBy(Category category)
        {
            return category.Products.Count >= _threshold;
        }
    }

    /// <summary>
    /// Specification để check category có chứa product cụ thể không
    /// </summary>
    public class ContainsProductSpecification : ICategorySpecification
    {
        private readonly Product _product;

        public ContainsProductSpecification(Product product)
        {
            _product = product ?? throw CategoryError.ProductCannotBeNull();
        }

        public bool IsSatisfiedBy(Category category)
        {
            return category.Products.Any(p => p.Id == _product.Id);
        }
    }

    /// <summary>
    /// Specification để check category name theo pattern
    /// </summary>
    public class CategoryNamePatternSpecification : ICategorySpecification
    {
        private readonly string _pattern;

        public CategoryNamePatternSpecification(string pattern)
        {
            _pattern = !string.IsNullOrWhiteSpace(pattern) ? pattern.ToLowerInvariant() :
                throw new ArgumentException("Pattern cannot be null or empty");
        }

        public bool IsSatisfiedBy(Category category)
        {
            return category.Name.ToLowerInvariant().Contains(_pattern);
        }
    }

    /// <summary>
    /// Specification để check category có popular không (nhiều orders)
    /// </summary>
    public class IsPopularCategorySpecification : ICategorySpecification
    {
        private readonly int _orderThreshold;

        public IsPopularCategorySpecification(int orderThreshold = 100)
        {
            if (orderThreshold <= 0)
                throw new ArgumentException("Order threshold must be greater than zero");

            _orderThreshold = orderThreshold;
        }

        public bool IsSatisfiedBy(Category category)
        {
            var totalOrders = category.Products.SelectMany(p => p.OrderItems).Count();
            return totalOrders >= _orderThreshold;
        }
    }

    /// <summary>
    /// Specification để check category có active products không
    /// </summary>
    public class HasActiveProductsSpecification : ICategorySpecification
    {
        public bool IsSatisfiedBy(Category category)
        {
            return category.Products.Any(p => p.OrderItems.Any());
        }
    }

    /// <summary>
    /// Composite specifications để kết hợp nhiều conditions
    /// </summary>
    public static class Composite
    {
        public class AndSpecification : ICategorySpecification
        {
            private readonly ICategorySpecification _left;
            private readonly ICategorySpecification _right;

            public AndSpecification(ICategorySpecification left, ICategorySpecification right)
            {
                _left = left ?? throw new ArgumentNullException(nameof(left));
                _right = right ?? throw new ArgumentNullException(nameof(right));
            }

            public bool IsSatisfiedBy(Category category)
            {
                return _left.IsSatisfiedBy(category) && _right.IsSatisfiedBy(category);
            }
        }

        public class OrSpecification : ICategorySpecification
        {
            private readonly ICategorySpecification _left;
            private readonly ICategorySpecification _right;

            public OrSpecification(ICategorySpecification left, ICategorySpecification right)
            {
                _left = left ?? throw new ArgumentNullException(nameof(left));
                _right = right ?? throw new ArgumentNullException(nameof(right));
            }

            public bool IsSatisfiedBy(Category category)
            {
                return _left.IsSatisfiedBy(category) || _right.IsSatisfiedBy(category);
            }
        }

        public class NotSpecification : ICategorySpecification
        {
            private readonly ICategorySpecification _specification;

            public NotSpecification(ICategorySpecification specification)
            {
                _specification = specification ?? throw new ArgumentNullException(nameof(specification));
            }

            public bool IsSatisfiedBy(Category category)
            {
                return !_specification.IsSatisfiedBy(category);
            }
        }
    }
}

/// <summary>
/// Extension methods để dễ sử dụng specifications
/// </summary>
public static class CategorySpecificationExtensions
{
    public static CategorySpecifications.Composite.AndSpecification And(
        this CategorySpecifications.ICategorySpecification left,
        CategorySpecifications.ICategorySpecification right)
    {
        return new CategorySpecifications.Composite.AndSpecification(left, right);
    }

    public static CategorySpecifications.Composite.OrSpecification Or(
        this CategorySpecifications.ICategorySpecification left,
        CategorySpecifications.ICategorySpecification right)
    {
        return new CategorySpecifications.Composite.OrSpecification(left, right);
    }

    public static CategorySpecifications.Composite.NotSpecification Not(
        this CategorySpecifications.ICategorySpecification specification)
    {
        return new CategorySpecifications.Composite.NotSpecification(specification);
    }
}

