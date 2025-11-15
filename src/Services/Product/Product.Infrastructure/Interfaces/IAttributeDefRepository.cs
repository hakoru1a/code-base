using Contracts.Common.Interface;
using Product.Domain.Entities;

namespace Product.Infrastructure.Interfaces
{
    public interface IAttributeDefRepository : IRepositoryBaseAsync<AttributeDef, long>
    {
        Task<IEnumerable<AttributeDef>> GetActiveAttributesAsync();
    }
}