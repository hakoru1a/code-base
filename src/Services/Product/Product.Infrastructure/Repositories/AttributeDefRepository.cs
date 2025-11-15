using Contracts.Common.Interface;
using Product.Domain.Entities;
using Product.Infrastructure.Interfaces;
using Product.Infrastructure.Persistences;
using Infrastructure.Common.Repository;
using Microsoft.EntityFrameworkCore;

namespace Product.Infrastructure.Repositories
{
    public class AttributeDefRepository : RepositoryBaseAsync<AttributeDef, long, ProductContext>, IAttributeDefRepository
    {
        public AttributeDefRepository(ProductContext dbContext, IUnitOfWork<ProductContext> unitOfWork) : base(dbContext, unitOfWork)
        {
        }

        public async Task<IEnumerable<AttributeDef>> GetActiveAttributesAsync()
        {
            return await FindAll()
                .Where(a => a.IsActive)
                .OrderBy(a => a.DisplayOrder)
                .ToListAsync();
        }
    }
}