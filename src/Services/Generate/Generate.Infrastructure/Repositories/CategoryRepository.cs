using Contracts.Common.Interface;
using Generate.Domain.Categories;
using Generate.Domain.Categories.Interfaces;
using Generate.Infrastructure.Persistences;
using Infrastructure.Common.Repository;

namespace Generate.Infrastructure.Repositories
{
    /// <summary>
    /// Infrastructure implementation of Category repository
    /// Implements Domain contract while handling persistence concerns
    /// </summary>
    public class CategoryRepository : RepositoryBaseAsync<Category, long, GenerateContext>, ICategoryRepository
    {
        public CategoryRepository(GenerateContext dbContext, IUnitOfWork<GenerateContext> unitOfWork) : base(dbContext, unitOfWork)
        {
        }

        // Domain-specific methods can be implemented here
        // For example:
        // public async Task<IEnumerable<Category>> GetCategoriesWithProductsAsync()
        // {
        //     return await FindAll().Include(c => c.Products).ToListAsync();
        // }
    }
}
