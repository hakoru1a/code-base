using Generate.Domain.Orders.Interfaces;
using Generate.Domain.Products.Interfaces;
using MediatR;
using OrderEntity = Generate.Domain.Orders.Order;

namespace Generate.Application.Features.Orders.Commands.CreateOrder
{
    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, long>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;

        public CreateOrderCommandHandler(IOrderRepository orderRepository, IProductRepository productRepository)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
        }

        public async Task<long> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            // Use DDD factory method
            var order = OrderEntity.Create(request.CustomerName);

            // Add order items using business methods (if provided)
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

            var result = await _orderRepository.CreateAsync(order);
            await _orderRepository.SaveChangesAsync();

            return result;
        }
    }
}