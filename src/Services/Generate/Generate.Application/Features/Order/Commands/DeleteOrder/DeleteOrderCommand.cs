using MediatR;

namespace Generate.Application.Features.Order.Commands.DeleteOrder
{
    public class DeleteOrderCommand : IRequest<bool>
    {
        public long Id { get; set; }
    }
}

