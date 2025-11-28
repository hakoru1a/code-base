using Contracts.Common.Interface;
using Generate.Domain.Entities.Products;

namespace Generate.Domain.Repositories;

/// <summary>
/// Repository contract for Product aggregate  
/// Part of Domain layer - defines what persistence capabilities are needed
/// </summary>
public interface IProductRepository : IRepositoryBaseAsync<Product, long>
{
    // Domain-specific repository methods can be added here
    // For example:
    // Task<IEnumerable<Product>> GetProductsByCategoryAsync(long categoryId);
    // Task<IEnumerable<Product>> GetProductsWithOrderItemsAsync();
}
