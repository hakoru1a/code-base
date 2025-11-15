using Contracts.Common.Interface;
using Product.Domain.Entities;

namespace Product.Infrastructure.Interfaces
{
    public interface IProductVariantRepository : IRepositoryBaseAsync<ProductVariant, long>
    {
        Task<IEnumerable<ProductVariant>> GetVariantsByProductIdAsync(long productId);
        Task<ProductVariant?> GetVariantWithAttributesAsync(long variantId);
    }
}