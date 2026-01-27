using Contracts.Common.Interface;
using Infrastructure.Common.Repository;
using TLBIOMASS.Domain.Agencies;
using TLBIOMASS.Domain.Agencies.Interfaces;
using TLBIOMASS.Infrastructure.Persistences;

namespace TLBIOMASS.Infrastructure.Repositories;

public class AgencyRepository : RepositoryBaseAsync<Agency, int, TLBIOMASSContext>, IAgencyRepository
{
    public AgencyRepository(TLBIOMASSContext dbContext, IUnitOfWork<TLBIOMASSContext> unitOfWork) 
        : base(dbContext, unitOfWork)
    {
    }
}
