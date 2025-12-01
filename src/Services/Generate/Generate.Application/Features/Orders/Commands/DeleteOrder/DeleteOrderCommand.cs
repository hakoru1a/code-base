using MediatR;

namespace Generate.Application.Features.Orders.Commands.DeleteOrder
{
    public class DeleteOrderCommand : IRequest<bool>
    {
        public long Id { get; set; }
    }
}

