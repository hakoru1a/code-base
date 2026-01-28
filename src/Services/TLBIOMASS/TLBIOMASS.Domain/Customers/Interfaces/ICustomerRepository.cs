using Contracts.Common.Interface;

namespace TLBIOMASS.Domain.Customers.Interfaces;

public interface ICustomerRepository : IRepositoryBaseAsync<Customer, int>
{
}
