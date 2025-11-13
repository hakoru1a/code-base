using AutoMapper;
using Shared.DTOs.Order;
using Generate.Infrastructure.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Generate.Application.Features.Order.Queries.GetOrders
{
    public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, List<OrderResponseDto>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;

        public GetOrdersQueryHandler(IOrderRepository orderRepository, IMapper mapper)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
        }

        public async Task<List<OrderResponseDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
        {
            var orders = await _orderRepository.FindAll()
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ToListAsync(cancellationToken);

            return _mapper.Map<List<OrderResponseDto>>(orders);
        }
    }
}

