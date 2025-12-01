using MediatR;

namespace Generate.Application.Features.Products.Commands.CreateProduct
{
    public class CreateProductCommand : IRequest<long>
    {
        public string Name { get; set; } = string.Empty;
        public long? CategoryId { get; set; }
    }
}

