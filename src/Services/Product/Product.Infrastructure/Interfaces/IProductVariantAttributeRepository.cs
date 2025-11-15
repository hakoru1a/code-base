using Contracts.Common.Interface;
using Product.Domain.Entities;

namespace Product.Infrastructure.Interfaces
{
    public interface IProductVariantAttributeRepository : IRepositoryBaseAsync<ProductVariantAttribute, long>
    {
        Task<IEnumerable<ProductVariantAttribute>> GetAttributesByVariantIdAsync(long variantId);
    }
}