using Base.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Application.Feature.Product.Commands.CreateProduct
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
            var product = Domain.Entities.Product.Create(
                request.Name,
                request.Description,
                request.Price,
                request.Stock,
                request.SKU
            );

            var productId = await _productRepository.CreateAsync(product);
            return productId;
        }
    }
}

