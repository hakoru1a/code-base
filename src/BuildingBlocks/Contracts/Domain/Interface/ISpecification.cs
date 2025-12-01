namespace Contracts.Domain.Interface;

/// <summary>
/// Specification Pattern interface for domain entities
/// Encapsulates business queries and conditions
/// </summary>
/// <typeparam name="T">The entity type to check</typeparam>
public interface ISpecification<in T>
{
    /// <summary>
    /// Determines whether the specification is satisfied by the given entity
    /// </summary>
    /// <param name="entity">The entity to check</param>
    /// <returns>True if the specification is satisfied, false otherwise</returns>
    bool IsSatisfiedBy(T entity);
}

