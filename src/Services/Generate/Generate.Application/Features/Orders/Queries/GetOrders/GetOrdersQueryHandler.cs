using Mapster;
using Shared.DTOs.Order;
using Generate.Domain.Orders;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Generate.Application.Features.Orders.Queries.GetOrders
{
    public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, List<OrderResponseDto>>
    {
        private readonly IOrderRepository _orderRepository;

        public GetOrdersQueryHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<List<OrderResponseDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
        {
            var orders = await _orderRepository.FindAll()
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ToListAsync(cancellationToken);

            return orders.Adapt<List<OrderResponseDto>>();
        }
    }
}

