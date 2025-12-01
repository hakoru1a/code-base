using Contracts.Domain.Interface;
using Contracts.Domain.Specifications;

namespace Contracts.Domain.Specifications;

/// <summary>
/// Extension methods để dễ sử dụng specifications
/// </summary>
public static class SpecificationExtensions
{
    public static CompositeSpecification.AndSpecification<T> And<T>(
        this ISpecification<T> left,
        ISpecification<T> right)
    {
        return new CompositeSpecification.AndSpecification<T>(left, right);
    }

    public static CompositeSpecification.OrSpecification<T> Or<T>(
        this ISpecification<T> left,
        ISpecification<T> right)
    {
        return new CompositeSpecification.OrSpecification<T>(left, right);
    }

    public static CompositeSpecification.NotSpecification<T> Not<T>(
        this ISpecification<T> specification)
    {
        return new CompositeSpecification.NotSpecification<T>(specification);
    }
}

