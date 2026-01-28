using Infrastructure.Common.Repository;
using Contracts.Common.Interface;
using TLBIOMASS.Domain.Payments;
using TLBIOMASS.Domain.Payments.Interfaces;
using TLBIOMASS.Infrastructure.Persistences;

namespace TLBIOMASS.Infrastructure.Repositories;

public class WeighingTicketPaymentRepository : RepositoryBaseAsync<WeighingTicketPayment, int, TLBIOMASSContext>, IWeighingTicketPaymentRepository
{
    public WeighingTicketPaymentRepository(TLBIOMASSContext dbContext, IUnitOfWork<TLBIOMASSContext> unitOfWork) 
        : base(dbContext, unitOfWork)
    {
    }
}
