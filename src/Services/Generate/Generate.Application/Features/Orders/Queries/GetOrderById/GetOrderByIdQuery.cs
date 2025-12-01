using Shared.DTOs.Order;
using MediatR;

namespace Generate.Application.Features.Orders.Queries.GetOrderById
{
    public class GetOrderByIdQuery : IRequest<OrderResponseDto?>
    {
        public long Id { get; set; }
    }
}

