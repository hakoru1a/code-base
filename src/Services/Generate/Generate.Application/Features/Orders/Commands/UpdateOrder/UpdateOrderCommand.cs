using Shared.DTOs.Order;
using MediatR;

namespace Generate.Application.Features.Orders.Commands.UpdateOrder
{
    public class UpdateOrderCommand : IRequest<bool>
    {
        public long Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public List<OrderItemUpdateDto> OrderItems { get; set; } = new List<OrderItemUpdateDto>();
    }
}

