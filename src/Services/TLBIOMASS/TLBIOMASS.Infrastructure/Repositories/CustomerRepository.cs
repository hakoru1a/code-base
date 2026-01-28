using Contracts.Common.Interface;
using Infrastructure.Common.Repository;
using TLBIOMASS.Domain.Customers;
using TLBIOMASS.Domain.Customers.Interfaces;
using TLBIOMASS.Infrastructure.Persistences;

namespace TLBIOMASS.Infrastructure.Repositories;

public class CustomerRepository : RepositoryBaseAsync<Customer, int, TLBIOMASSContext>, ICustomerRepository
{
    public CustomerRepository(TLBIOMASSContext dbContext, IUnitOfWork<TLBIOMASSContext> unitOfWork) 
        : base(dbContext, unitOfWork)
    {
    }
}
