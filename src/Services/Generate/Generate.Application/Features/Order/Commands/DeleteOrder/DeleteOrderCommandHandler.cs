using Generate.Domain.Repositories;
using MediatR;

namespace Generate.Application.Features.Order.Commands.DeleteOrder
{
    public class DeleteOrderCommandHandler : IRequestHandler<DeleteOrderCommand, bool>
    {
        private readonly IOrderRepository _orderRepository;

        public DeleteOrderCommandHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<bool> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdAsync(request.Id);

            if (order == null)
                return false;

            await _orderRepository.DeleteAsync(order);
            await _orderRepository.SaveChangesAsync();

            return true;
        }
    }
}

