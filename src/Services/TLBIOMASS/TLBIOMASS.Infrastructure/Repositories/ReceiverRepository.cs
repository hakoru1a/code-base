using Infrastructure.Common.Repository;
using TLBIOMASS.Domain.Receivers;
using TLBIOMASS.Domain.Receivers.Interfaces;
using TLBIOMASS.Infrastructure.Persistences;
using Contracts.Common.Interface;

namespace TLBIOMASS.Infrastructure.Repositories;

public class ReceiverRepository : RepositoryBaseAsync<Receiver, int, TLBIOMASSContext>, IReceiverRepository
{
    public ReceiverRepository(TLBIOMASSContext dbContext, IUnitOfWork<TLBIOMASSContext> unitOfWork) : base(dbContext, unitOfWork)
    {
    }
}
