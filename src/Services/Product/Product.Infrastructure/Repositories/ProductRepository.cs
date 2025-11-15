using Contracts.Common.Interface;
using Product.Infrastructure.Interfaces;
using Product.Infrastructure.Persistences;
using Infrastructure.Common.Repository;
using Microsoft.EntityFrameworkCore;
using ProductEntity = Product.Domain.Entities.Product;

namespace Product.Infrastructure.Repositories
{
    public class ProductRepository : RepositoryBaseAsync<ProductEntity, long, ProductContext>, IProductRepository
    {
        public ProductRepository(ProductContext dbContext, IUnitOfWork<ProductContext> unitOfWork) : base(dbContext, unitOfWork)
        {
        }

        public async Task<IEnumerable<ProductEntity>> GetProductsWithVariantsAsync()
        {
            return await FindAll()
                .Include(p => p.Variants)
                .ThenInclude(v => v.Attributes)
                .ThenInclude(a => a.AttributeDef)
                .ToListAsync();
        }

        public async Task<ProductEntity?> GetProductWithVariantsAsync(long productId)
        {
            return await FindAll()
                .Include(p => p.Variants)
                .ThenInclude(v => v.Attributes)
                .ThenInclude(a => a.AttributeDef)
                .FirstOrDefaultAsync(p => p.Id == productId);
        }
    }
}