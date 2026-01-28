using Infrastructure.Common.Repository;
using Contracts.Common.Interface;
using TLBIOMASS.Domain.Payments;
using TLBIOMASS.Domain.Payments.Interfaces;
using TLBIOMASS.Infrastructure.Persistences;

namespace TLBIOMASS.Infrastructure.Repositories;

public class PaymentDetailRepository : RepositoryBaseAsync<PaymentDetail, int, TLBIOMASSContext>, IPaymentDetailRepository
{
    public PaymentDetailRepository(TLBIOMASSContext dbContext, IUnitOfWork<TLBIOMASSContext> unitOfWork) 
        : base(dbContext, unitOfWork)
    {
    }
}
