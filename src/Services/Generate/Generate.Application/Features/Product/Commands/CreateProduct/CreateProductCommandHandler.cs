using Generate.Infrastructure.Interfaces;
using MediatR;

namespace Generate.Application.Features.Product.Commands.CreateProduct
{
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, long>
    {
        private readonly IProductRepository _productRepository;

        public CreateProductCommandHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<long> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var product = new Domain.Entities.Product
            {
                Name = request.Name,
                CategoryId = request.CategoryId
            };

            var result = await _productRepository.CreateAsync(product);
            await _productRepository.SaveChangesAsync();

            return result;
        }
    }
}

