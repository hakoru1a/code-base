using Shared.DTOs.Order;
using MediatR;

namespace Generate.Application.Features.Order.Commands.CreateOrder
{
    public class CreateOrderCommand : IRequest<long>
    {
        public string CustomerName { get; set; } = string.Empty;
        public List<OrderItemCreateDto> OrderItems { get; set; } = new List<OrderItemCreateDto>();
    }
}

