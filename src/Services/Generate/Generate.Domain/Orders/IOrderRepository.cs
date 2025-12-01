using Contracts.Common.Interface;
using Generate.Domain.Orders;

namespace Generate.Domain.Orders;

/// <summary>
/// Repository contract for Order aggregate
/// Part of Domain layer - defines what persistence capabilities are needed  
/// </summary>
public interface IOrderRepository : IRepositoryBaseAsync<Order, long>
{
    // Domain-specific repository methods can be added here
    // For example:
    // Task<IEnumerable<Order>> GetOrdersByCustomerAsync(string customerName);
    // Task<IEnumerable<Order>> GetOrdersWithItemsAsync();
}

