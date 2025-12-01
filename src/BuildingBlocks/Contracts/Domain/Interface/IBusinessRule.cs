namespace Contracts.Domain.Interface;

/// <summary>
/// Interface for all business rules in the system.
/// Business rules encapsulate domain logic that can be validated independently.
/// </summary>
public interface IBusinessRule
{
    /// <summary>
    /// Determines whether this business rule is broken/violated.
    /// </summary>
    /// <returns>True if the rule is broken, false otherwise.</returns>
    bool IsBroken();

    /// <summary>
    /// Gets the human-readable error message when the rule is broken.
    /// </summary>
    string Message { get; }

    /// <summary>
    /// Gets the error code for programmatic handling and localization.
    /// Format: {Entity}.{RuleName} (e.g., "Product.NameRequired", "Order.MaxItemsExceeded")
    /// </summary>
    string Code { get; }
}
