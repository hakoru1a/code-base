using Shared.DTOs.Order;
using MediatR;

namespace Generate.Application.Features.Order.Queries.GetOrders
{
    public class GetOrdersQuery : IRequest<List<OrderResponseDto>>
    {
    }
}

