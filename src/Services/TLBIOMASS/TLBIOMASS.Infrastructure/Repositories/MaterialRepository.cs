using Contracts.Common.Interface;
using Infrastructure.Common.Repository;
using TLBIOMASS.Domain.Materials;
using TLBIOMASS.Domain.Materials.Interfaces;
using TLBIOMASS.Infrastructure.Persistences;

namespace TLBIOMASS.Infrastructure.Repositories;

public class MaterialRepository : RepositoryBaseAsync<Material, int, TLBIOMASSContext>, IMaterialRepository
{
    public MaterialRepository(TLBIOMASSContext dbContext, IUnitOfWork<TLBIOMASSContext> unitOfWork) 
        : base(dbContext, unitOfWork)
    {
    }
}
