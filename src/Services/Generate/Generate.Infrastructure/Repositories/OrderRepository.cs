using Contracts.Common.Interface;
using Generate.Domain.Orders;
using Generate.Infrastructure.Persistences;
using Infrastructure.Common.Repository;

namespace Generate.Infrastructure.Repositories
{
    /// <summary>
    /// Infrastructure implementation of Order repository
    /// Implements Domain contract while handling persistence concerns
    /// </summary>
    public class OrderRepository : RepositoryBaseAsync<Order, long, GenerateContext>, IOrderRepository
    {
        public OrderRepository(GenerateContext dbContext, IUnitOfWork<GenerateContext> unitOfWork) : base(dbContext, unitOfWork)
        {
        }

        // Domain-specific methods can be implemented here
        // For example:
        // public async Task<IEnumerable<Order>> GetOrdersByCustomerAsync(string customerName)
        // {
        //     return await FindAll(o => o.CustomerName == customerName).ToListAsync();
        // }
    }
}
