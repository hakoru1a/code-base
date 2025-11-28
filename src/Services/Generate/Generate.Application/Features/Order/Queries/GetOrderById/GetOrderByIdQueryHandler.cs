using Mapster;
using Shared.DTOs.Order;
using Generate.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Generate.Application.Features.Order.Queries.GetOrderById
{
    public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderResponseDto?>
    {
        private readonly IOrderRepository _orderRepository;

        public GetOrderByIdQueryHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<OrderResponseDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.FindAll()
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

            return order?.Adapt<OrderResponseDto>();
        }
    }
}

