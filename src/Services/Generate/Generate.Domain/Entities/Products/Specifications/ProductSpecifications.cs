using Generate.Domain.Entities.Categories;
using Generate.Domain.Entities.Orders.ValueObject;

namespace Generate.Domain.Entities.Products.Specifications;

/// <summary>
/// Specification Pattern cho Product entity
/// Tách riêng business queries và conditions ra khỏi entity
/// </summary>
public static class ProductSpecifications
{
    /// <summary>
    /// Base interface cho Product specifications
    /// </summary>
    public interface IProductSpecification
    {
        bool IsSatisfiedBy(Product product);
    }

    /// <summary>
    /// Specification để check product có thể bị xóa không
    /// </summary>
    public class CanBeDeletedSpecification : IProductSpecification
    {
        public bool IsSatisfiedBy(Product product)
        {
            return !product.OrderItems.Any();
        }
    }

    /// <summary>
    /// Specification để check product có trong category không
    /// </summary>
    public class IsInCategorySpecification : IProductSpecification
    {
        public bool IsSatisfiedBy(Product product)
        {
            return product.Category != null;
        }
    }

    /// <summary>
    /// Specification để check product thuộc về category cụ thể
    /// </summary>
    public class BelongsToCategorySpecification : IProductSpecification
    {
        private readonly Category _category;

        public BelongsToCategorySpecification(Category category)
        {
            _category = category ?? throw ProductError.CategoryCannotBeNull();
        }

        public bool IsSatisfiedBy(Product product)
        {
            return product.Category != null && product.Category.Id == _category.Id;
        }
    }

    /// <summary>
    /// Specification để check product có phải là popular product không
    /// </summary>
    public class IsPopularProductSpecification : IProductSpecification
    {
        private readonly int _orderThreshold;

        public IsPopularProductSpecification(int orderThreshold = 10)
        {
            if (orderThreshold <= 0)
                throw new ArgumentException("Order threshold must be greater than zero");
            
            _orderThreshold = orderThreshold;
        }

        public bool IsSatisfiedBy(Product product)
        {
            return product.OrderItems.Count >= _orderThreshold;
        }
    }

    /// <summary>
    /// Specification để check product có high volume không (quantity lớn)
    /// </summary>
    public class IsHighVolumeProductSpecification : IProductSpecification
    {
        private readonly decimal _volumeThreshold;

        public IsHighVolumeProductSpecification(decimal volumeThreshold = 100)
        {
            if (volumeThreshold <= 0)
                throw new ArgumentException("Volume threshold must be greater than zero");
            
            _volumeThreshold = volumeThreshold;
        }

        public bool IsSatisfiedBy(Product product)
        {
            var totalVolume = product.OrderItems.Sum(oi => oi.Quantity);
            return totalVolume >= _volumeThreshold;
        }
    }

    /// <summary>
    /// Specification để check product name theo pattern
    /// </summary>
    public class ProductNamePatternSpecification : IProductSpecification
    {
        private readonly string _pattern;

        public ProductNamePatternSpecification(string pattern)
        {
            _pattern = !string.IsNullOrWhiteSpace(pattern) ? pattern.ToLowerInvariant() : 
                throw new ArgumentException("Pattern cannot be null or empty");
        }

        public bool IsSatisfiedBy(Product product)
        {
            return product.Name.ToLowerInvariant().Contains(_pattern);
        }
    }

    /// <summary>
    /// Specification để check product có order items không
    /// </summary>
    public class HasOrderItemsSpecification : IProductSpecification
    {
        public bool IsSatisfiedBy(Product product)
        {
            return product.OrderItems.Any();
        }
    }

    /// <summary>
    /// Specification để check product có product detail không
    /// </summary>
    public class HasProductDetailSpecification : IProductSpecification
    {
        public bool IsSatisfiedBy(Product product)
        {
            return product.ProductDetail != null;
        }
    }

    /// <summary>
    /// Specification để check product có orders trong date range
    /// </summary>
    public class HasOrdersInDateRangeSpecification : IProductSpecification
    {
        private readonly DateTime _fromDate;
        private readonly DateTime _toDate;

        public HasOrdersInDateRangeSpecification(DateTime fromDate, DateTime toDate)
        {
            if (fromDate > toDate)
                throw new ArgumentException("From date must be less than or equal to to date");
            
            _fromDate = fromDate;
            _toDate = toDate;
        }

        public bool IsSatisfiedBy(Product product)
        {
            return product.OrderItems.Any(oi => 
                oi.CreatedDate >= _fromDate && oi.CreatedDate <= _toDate);
        }
    }

    /// <summary>
    /// Composite specifications để kết hợp nhiều conditions
    /// </summary>
    public static class Composite
    {
        public class AndSpecification : IProductSpecification
        {
            private readonly IProductSpecification _left;
            private readonly IProductSpecification _right;

            public AndSpecification(IProductSpecification left, IProductSpecification right)
            {
                _left = left ?? throw new ArgumentNullException(nameof(left));
                _right = right ?? throw new ArgumentNullException(nameof(right));
            }

            public bool IsSatisfiedBy(Product product)
            {
                return _left.IsSatisfiedBy(product) && _right.IsSatisfiedBy(product);
            }
        }

        public class OrSpecification : IProductSpecification
        {
            private readonly IProductSpecification _left;
            private readonly IProductSpecification _right;

            public OrSpecification(IProductSpecification left, IProductSpecification right)
            {
                _left = left ?? throw new ArgumentNullException(nameof(left));
                _right = right ?? throw new ArgumentNullException(nameof(right));
            }

            public bool IsSatisfiedBy(Product product)
            {
                return _left.IsSatisfiedBy(product) || _right.IsSatisfiedBy(product);
            }
        }

        public class NotSpecification : IProductSpecification
        {
            private readonly IProductSpecification _specification;

            public NotSpecification(IProductSpecification specification)
            {
                _specification = specification ?? throw new ArgumentNullException(nameof(specification));
            }

            public bool IsSatisfiedBy(Product product)
            {
                return !_specification.IsSatisfiedBy(product);
            }
        }
    }
}

/// <summary>
/// Extension methods để dễ sử dụng specifications
/// </summary>
public static class ProductSpecificationExtensions
{
    public static ProductSpecifications.Composite.AndSpecification And(
        this ProductSpecifications.IProductSpecification left, 
        ProductSpecifications.IProductSpecification right)
    {
        return new ProductSpecifications.Composite.AndSpecification(left, right);
    }

    public static ProductSpecifications.Composite.OrSpecification Or(
        this ProductSpecifications.IProductSpecification left, 
        ProductSpecifications.IProductSpecification right)
    {
        return new ProductSpecifications.Composite.OrSpecification(left, right);
    }

    public static ProductSpecifications.Composite.NotSpecification Not(
        this ProductSpecifications.IProductSpecification specification)
    {
        return new ProductSpecifications.Composite.NotSpecification(specification);
    }
}

