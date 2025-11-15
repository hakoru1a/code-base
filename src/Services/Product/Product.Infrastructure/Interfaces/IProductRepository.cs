using Contracts.Common.Interface;
using ProductEntity = Product.Domain.Entities.Product;

namespace Product.Infrastructure.Interfaces
{
    public interface IProductRepository : IRepositoryBaseAsync<ProductEntity, long>
    {
        Task<IEnumerable<ProductEntity>> GetProductsWithVariantsAsync();
        Task<ProductEntity?> GetProductWithVariantsAsync(long productId);
    }
}