using Shared.DTOs.Order;
using MediatR;

namespace Generate.Application.Features.Order.Queries.GetOrderById
{
    public class GetOrderByIdQuery : IRequest<OrderResponseDto?>
    {
        public long Id { get; set; }
    }
}

