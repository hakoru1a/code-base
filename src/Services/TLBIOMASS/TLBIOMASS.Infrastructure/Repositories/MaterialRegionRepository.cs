using Contracts.Common.Interface;
using Infrastructure.Common.Repository;
using TLBIOMASS.Domain.MaterialRegions;
using TLBIOMASS.Domain.MaterialRegions.Interfaces;
using TLBIOMASS.Infrastructure.Persistences;

namespace TLBIOMASS.Infrastructure.Repositories;

public class MaterialRegionRepository : RepositoryBaseAsync<MaterialRegion, int, TLBIOMASSContext>, IMaterialRegionRepository
{
    public MaterialRegionRepository(TLBIOMASSContext dbContext, IUnitOfWork<TLBIOMASSContext> unitOfWork) 
        : base(dbContext, unitOfWork)
    {
    }
}
