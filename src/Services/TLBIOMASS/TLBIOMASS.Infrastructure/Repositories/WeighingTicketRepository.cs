using Contracts.Common.Interface;
using Infrastructure.Common.Repository;
using TLBIOMASS.Domain.WeighingTickets;
using TLBIOMASS.Domain.WeighingTickets.Interfaces;
using TLBIOMASS.Infrastructure.Persistences;

namespace TLBIOMASS.Infrastructure.Repositories;

public class WeighingTicketRepository : RepositoryBaseAsync<WeighingTicket, int, TLBIOMASSContext>, IWeighingTicketRepository
{
    public WeighingTicketRepository(TLBIOMASSContext dbContext, IUnitOfWork<TLBIOMASSContext> unitOfWork) 
        : base(dbContext, unitOfWork)
    {
    }
}
