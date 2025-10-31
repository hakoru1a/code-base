using MediatR;

namespace Generate.Application.Features.Product.Commands.UpdateProduct
{
    public class UpdateProductCommand : IRequest<bool>
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public long? CategoryId { get; set; }
    }
}

