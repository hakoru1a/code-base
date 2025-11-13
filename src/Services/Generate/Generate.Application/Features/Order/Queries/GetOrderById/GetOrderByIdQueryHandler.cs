using AutoMapper;
using Shared.DTOs.Order;
using Generate.Infrastructure.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Generate.Application.Features.Order.Queries.GetOrderById
{
    public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderResponseDto?>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;

        public GetOrderByIdQueryHandler(IOrderRepository orderRepository, IMapper mapper)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
        }

        public async Task<OrderResponseDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.FindAll()
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

            return order == null ? null : _mapper.Map<OrderResponseDto>(order);
        }
    }
}

