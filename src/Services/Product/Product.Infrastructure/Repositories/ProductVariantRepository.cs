using Contracts.Common.Interface;
using Product.Domain.Entities;
using Product.Infrastructure.Interfaces;
using Product.Infrastructure.Persistences;
using Infrastructure.Common.Repository;
using Microsoft.EntityFrameworkCore;

namespace Product.Infrastructure.Repositories
{
    public class ProductVariantRepository : RepositoryBaseAsync<ProductVariant, long, ProductContext>, IProductVariantRepository
    {
        public ProductVariantRepository(ProductContext dbContext, IUnitOfWork<ProductContext> unitOfWork) : base(dbContext, unitOfWork)
        {
        }

        public async Task<IEnumerable<ProductVariant>> GetVariantsByProductIdAsync(long productId)
        {
            return await FindAll()
                .Where(v => v.ProductId == productId)
                .Include(v => v.Attributes)
                .ThenInclude(a => a.AttributeDef)
                .ToListAsync();
        }

        public async Task<ProductVariant?> GetVariantWithAttributesAsync(long variantId)
        {
            return await FindAll()
                .Include(v => v.Attributes)
                .ThenInclude(a => a.AttributeDef)
                .FirstOrDefaultAsync(v => v.Id == variantId);
        }
    }
}