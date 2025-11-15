using Contracts.Common.Interface;
using Product.Domain.Entities;
using Product.Infrastructure.Interfaces;
using Product.Infrastructure.Persistences;
using Infrastructure.Common.Repository;
using Microsoft.EntityFrameworkCore;

namespace Product.Infrastructure.Repositories
{
    public class ProductVariantAttributeRepository : RepositoryBaseAsync<ProductVariantAttribute, long, ProductContext>, IProductVariantAttributeRepository
    {
        public ProductVariantAttributeRepository(ProductContext dbContext, IUnitOfWork<ProductContext> unitOfWork) : base(dbContext, unitOfWork)
        {
        }

        public async Task<IEnumerable<ProductVariantAttribute>> GetAttributesByVariantIdAsync(long variantId)
        {
            return await FindAll()
                .Where(a => a.ProductVariantId == variantId)
                .Include(a => a.AttributeDef)
                .ToListAsync();
        }
    }
}