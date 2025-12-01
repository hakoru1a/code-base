using Shared.DTOs.Order;
using MediatR;

namespace Generate.Application.Features.Orders.Queries.GetOrders
{
    public class GetOrdersQuery : IRequest<List<OrderResponseDto>>
    {
    }
}

