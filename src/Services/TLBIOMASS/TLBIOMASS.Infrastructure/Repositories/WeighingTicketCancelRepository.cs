using Contracts.Common.Interface;
using Infrastructure.Common.Repository;
using TLBIOMASS.Domain.WeighingTicketCancels;
using TLBIOMASS.Domain.WeighingTicketCancels.Interfaces;
using TLBIOMASS.Infrastructure.Persistences;

namespace TLBIOMASS.Infrastructure.Repositories;

public class WeighingTicketCancelRepository : RepositoryBaseAsync<WeighingTicketCancel, int, TLBIOMASSContext>, IWeighingTicketCancelRepository
{
    public WeighingTicketCancelRepository(TLBIOMASSContext dbContext, IUnitOfWork<TLBIOMASSContext> unitOfWork) 
        : base(dbContext, unitOfWork)
    {
    }
}
