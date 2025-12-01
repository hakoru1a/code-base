using Generate.Domain.Orders;
using Generate.Domain.Products;
using MediatR;

namespace Generate.Application.Features.Orders.Commands.UpdateOrder
{
    public class UpdateOrderCommandHandler : IRequestHandler<UpdateOrderCommand, bool>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;

        public UpdateOrderCommandHandler(IOrderRepository orderRepository, IProductRepository productRepository)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
        }

        public async Task<bool> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdAsync(request.Id);

            if (order == null)
                return false;

            // Use business method for customer name update
            order.UpdateCustomerName(request.CustomerName);

            // Handle order items using business methods
            // Clear existing items and add new ones
            order.ClearOrderItems();

            if (request.OrderItems?.Any() == true)
            {
                foreach (var item in request.OrderItems)
                {
                    var product = await _productRepository.GetByIdAsync(item.ProductId);
                    if (product != null)
                    {
                        order.AddOrderItem(product, item.Quantity);
                    }
                }
            }

            await _orderRepository.UpdateAsync(order);
            await _orderRepository.SaveChangesAsync();

            return true;
        }
    }
}