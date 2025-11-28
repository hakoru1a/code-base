using Contracts.Common.Interface;
using Generate.Domain.Entities.Products;
using Generate.Domain.Repositories;
using Generate.Infrastructure.Persistences;
using Infrastructure.Common.Repository;

namespace Generate.Infrastructure.Repositories
{
    /// <summary>
    /// Infrastructure implementation of Product repository
    /// Implements Domain contract while handling persistence concerns
    /// </summary>
    public class ProductRepository : RepositoryBaseAsync<Product, long, GenerateContext>, IProductRepository
    {
        public ProductRepository(GenerateContext dbContext, IUnitOfWork<GenerateContext> unitOfWork) : base(dbContext, unitOfWork)
        {
        }

        // Domain-specific methods can be implemented here
        // For example:
        // public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(long categoryId)
        // {
        //     return await FindAll(p => p.Category != null && p.Category.Id == categoryId).ToListAsync();
        // }
    }
}
