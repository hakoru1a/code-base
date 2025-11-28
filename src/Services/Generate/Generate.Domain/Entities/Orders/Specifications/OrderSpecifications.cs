using Generate.Domain.Entities.Orders.ValueObject;
using Generate.Domain.Entities.Products;

namespace Generate.Domain.Entities.Orders.Specifications;

/// <summary>
/// Specification Pattern cho Order entity
/// Tách riêng business queries và conditions ra khỏi entity
/// </summary>
public static class OrderSpecifications
{
    /// <summary>
    /// Base interface cho Order specifications
    /// </summary>
    public interface IOrderSpecification
    {
        bool IsSatisfiedBy(Order order);
    }

    /// <summary>
    /// Specification để check order có thể bị xóa không
    /// </summary>
    public class CanBeDeletedSpecification : IOrderSpecification
    {
        public bool IsSatisfiedBy(Order order)
        {
            return !order.OrderItems.Any();
        }
    }

    /// <summary>
    /// Specification để check order có items không
    /// </summary>
    public class HasItemsSpecification : IOrderSpecification
    {
        public bool IsSatisfiedBy(Order order)
        {
            return order.OrderItems.Any();
        }
    }

    /// <summary>
    /// Specification để check order có phải là large order không
    /// </summary>
    public class IsLargeOrderSpecification : IOrderSpecification
    {
        private readonly int _threshold;

        public IsLargeOrderSpecification(int threshold = 50)
        {
            if (threshold <= 0)
                throw OrderError.InvalidThreshold(threshold);
            
            _threshold = threshold;
        }

        public bool IsSatisfiedBy(Order order)
        {
            var totalItems = order.OrderItems.Sum(oi => oi.Quantity);
            return totalItems >= _threshold;
        }
    }

    /// <summary>
    /// Specification để check order có chứa product cụ thể không
    /// </summary>
    public class ContainsProductSpecification : IOrderSpecification
    {
        private readonly Product _product;

        public ContainsProductSpecification(Product product)
        {
            _product = product ?? throw OrderError.ProductCannotBeNull();
        }

        public bool IsSatisfiedBy(Order order)
        {
            return order.OrderItems.Any(oi => ReferenceEquals(oi.Product, _product));
        }
    }

    /// <summary>
    /// Specification để check order value trong khoảng
    /// </summary>
    public class OrderValueRangeSpecification : IOrderSpecification
    {
        private readonly decimal _minValue;
        private readonly decimal _maxValue;

        public OrderValueRangeSpecification(decimal minValue, decimal maxValue)
        {
            if (minValue < 0) throw new ArgumentException("Min value cannot be negative");
            if (maxValue < minValue) throw new ArgumentException("Max value must be greater than min value");
            
            _minValue = minValue;
            _maxValue = maxValue;
        }

        public bool IsSatisfiedBy(Order order)
        {
            // Tạm thời dùng total quantity, sau này có thể thay bằng pricing logic
            var totalValue = order.OrderItems.Sum(oi => oi.Quantity);
            return totalValue >= _minValue && totalValue <= _maxValue;
        }
    }

    /// <summary>
    /// Specification để check customer name theo pattern
    /// </summary>
    public class CustomerNamePatternSpecification : IOrderSpecification
    {
        private readonly string _pattern;

        public CustomerNamePatternSpecification(string pattern)
        {
            _pattern = !string.IsNullOrWhiteSpace(pattern) ? pattern.ToLowerInvariant() : 
                throw new ArgumentException("Pattern cannot be null or empty");
        }

        public bool IsSatisfiedBy(Order order)
        {
            return order.CustomerName.ToLowerInvariant().Contains(_pattern);
        }
    }

    /// <summary>
    /// Composite specifications để kết hợp nhiều conditions
    /// </summary>
    public static class Composite
    {
        public class AndSpecification : IOrderSpecification
        {
            private readonly IOrderSpecification _left;
            private readonly IOrderSpecification _right;

            public AndSpecification(IOrderSpecification left, IOrderSpecification right)
            {
                _left = left ?? throw new ArgumentNullException(nameof(left));
                _right = right ?? throw new ArgumentNullException(nameof(right));
            }

            public bool IsSatisfiedBy(Order order)
            {
                return _left.IsSatisfiedBy(order) && _right.IsSatisfiedBy(order);
            }
        }

        public class OrSpecification : IOrderSpecification
        {
            private readonly IOrderSpecification _left;
            private readonly IOrderSpecification _right;

            public OrSpecification(IOrderSpecification left, IOrderSpecification right)
            {
                _left = left ?? throw new ArgumentNullException(nameof(left));
                _right = right ?? throw new ArgumentNullException(nameof(right));
            }

            public bool IsSatisfiedBy(Order order)
            {
                return _left.IsSatisfiedBy(order) || _right.IsSatisfiedBy(order);
            }
        }

        public class NotSpecification : IOrderSpecification
        {
            private readonly IOrderSpecification _specification;

            public NotSpecification(IOrderSpecification specification)
            {
                _specification = specification ?? throw new ArgumentNullException(nameof(specification));
            }

            public bool IsSatisfiedBy(Order order)
            {
                return !_specification.IsSatisfiedBy(order);
            }
        }
    }
}

/// <summary>
/// Extension methods để dễ sử dụng specifications
/// </summary>
public static class OrderSpecificationExtensions
{
    public static OrderSpecifications.Composite.AndSpecification And(
        this OrderSpecifications.IOrderSpecification left, 
        OrderSpecifications.IOrderSpecification right)
    {
        return new OrderSpecifications.Composite.AndSpecification(left, right);
    }

    public static OrderSpecifications.Composite.OrSpecification Or(
        this OrderSpecifications.IOrderSpecification left, 
        OrderSpecifications.IOrderSpecification right)
    {
        return new OrderSpecifications.Composite.OrSpecification(left, right);
    }

    public static OrderSpecifications.Composite.NotSpecification Not(
        this OrderSpecifications.IOrderSpecification specification)
    {
        return new OrderSpecifications.Composite.NotSpecification(specification);
    }
}
