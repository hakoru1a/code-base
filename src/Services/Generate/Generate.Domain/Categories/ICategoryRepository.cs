using Contracts.Common.Interface;
using Generate.Domain.Categories;

namespace Generate.Domain.Categories;

/// <summary>
/// Repository contract for Category aggregate
/// Part of Domain layer - defines what persistence capabilities are needed
/// </summary>
public interface ICategoryRepository : IRepositoryBaseAsync<Category, long>
{
    // Domain-specific repository methods can be added here
    // For example:
    // Task<IEnumerable<Category>> GetCategoriesWithProductsAsync();
    // Task<Category?> GetCategoryByNameAsync(string name);
}

