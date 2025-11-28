using System;
using System.Collections.Generic;
using System.Linq;

namespace Contracts.Domain;

/// <summary>
/// Static helper class for Entity Management operations
/// Provides common patterns for Add, Remove, Update operations
/// </summary>
public static class EntityManagementHelper
{
    /// <summary>
    /// Template method for Add operations
    /// </summary>
    public static void ExecuteAdd<TItem, TCollection>(TCollection collection, TItem item,
        Action<TItem> validate,
        Action<TCollection, TItem> validateNotExists,
        Action<TCollection>? validateConstraints = null)
        where TCollection : ICollection<TItem>
    {
        // Step 1: Validate item
        validate(item);

        // Step 2: Check if item already exists
        validateNotExists(collection, item);

        // Step 3: Validate business constraints
        validateConstraints?.Invoke(collection);

        // Step 4: Execute add
        collection.Add(item);
    }

    /// <summary>
    /// Template method for Remove operations
    /// </summary>
    public static void ExecuteRemove<TItem, TCollection>(TCollection collection, TItem item,
        Action<TItem> validate,
        Action<TCollection, TItem> validateExists)
        where TCollection : ICollection<TItem>
    {
        // Step 1: Validate item
        validate(item);

        // Step 2: Check if item exists
        validateExists(collection, item);

        // Step 3: Execute remove
        collection.Remove(item);
    }
}

/// <summary>
/// Static helper class for Entity Analytics operations
/// Provides common patterns for Calculate, Count, Analyze operations
/// </summary>
public static class EntityAnalyticsHelper
{
    /// <summary>
    /// Template method for counting operations
    /// </summary>
    public static int ExecuteCount<T>(IEnumerable<T> collection, Func<T, bool>? predicate = null)
    {
        return predicate == null ? collection.Count() : collection.Count(predicate);
    }

    /// <summary>
    /// Template method for sum operations
    /// </summary>
    public static TResult ExecuteSum<T, TResult>(IEnumerable<T> collection, Func<T, TResult> selector)
        where TResult : struct
    {
        return collection.Aggregate(default(TResult), (acc, item) =>
        {
            dynamic a = acc;
            dynamic b = selector(item);
            return a + b;
        });
    }

    /// <summary>
    /// Template method for threshold-based analysis
    /// </summary>
    public static bool ExecuteThresholdCheck<T>(IEnumerable<T> collection, Func<IEnumerable<T>, int> calculator, int threshold)
    {
        if (threshold <= 0)
            throw new ArgumentException("Threshold must be greater than zero", nameof(threshold));

        return calculator(collection) >= threshold;
    }
}

/// <summary>
/// Static helper class for Entity Query operations
/// Provides common patterns for Find, Get, Contains operations
/// </summary>
public static class EntityQueryHelper
{
    /// <summary>
    /// Template method for finding items by predicate
    /// </summary>
    public static IReadOnlyList<T> ExecuteFind<T>(IEnumerable<T> collection, Func<T, bool> predicate)
    {
        return collection.Where(predicate).ToList().AsReadOnly();
    }

    /// <summary>
    /// Template method for getting filtered items
    /// </summary>
    public static IReadOnlyList<T> ExecuteGet<T>(IEnumerable<T> collection, Func<T, bool>? predicate = null)
    {
        var query = predicate == null ? collection : collection.Where(predicate);
        return query.ToList().AsReadOnly();
    }

    /// <summary>
    /// Template method for contains operations
    /// </summary>
    public static bool ExecuteContains<T>(IEnumerable<T> collection, Func<T, bool> predicate)
    {
        return collection.Any(predicate);
    }
}

/// <summary>
/// Static helper class for Entity Lifecycle operations
/// Provides common patterns for Can*, Has*, Is* operations
/// </summary>
public static class EntityLifecycleHelper
{
    /// <summary>
    /// Template method for "Can" operations (CanBeDeleted, CanBeUpdated)
    /// </summary>
    public static bool ExecuteCanOperation<T>(IEnumerable<T> collection, Func<IEnumerable<T>, bool> canRule)
    {
        return canRule(collection);
    }

    /// <summary>
    /// Template method for "Has" operations (HasItems, HasActiveItems)
    /// </summary>
    public static bool ExecuteHasOperation<T>(IEnumerable<T> collection, Func<T, bool>? predicate = null)
    {
        return predicate == null ? collection.Any() : collection.Any(predicate);
    }

    /// <summary>
    /// Template method for "Is" operations (IsLarge, IsActive)
    /// </summary>
    public static bool ExecuteIsOperation<T>(IEnumerable<T> collection, Func<IEnumerable<T>, bool> isRule)
    {
        return isRule(collection);
    }
}
