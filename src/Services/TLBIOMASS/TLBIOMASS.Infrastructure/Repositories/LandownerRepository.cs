using Contracts.Common.Interface;
using Infrastructure.Common.Repository;
using TLBIOMASS.Domain.Landowners;
using TLBIOMASS.Domain.Landowners.Interfaces;
using TLBIOMASS.Infrastructure.Persistences;

namespace TLBIOMASS.Infrastructure.Repositories;

public class LandownerRepository : RepositoryBaseAsync<Landowner, int, TLBIOMASSContext>, ILandownerRepository
{
    public LandownerRepository(TLBIOMASSContext dbContext, IUnitOfWork<TLBIOMASSContext> unitOfWork) 
        : base(dbContext, unitOfWork)
    {
    }
}
