using MediatR;

namespace Generate.Application.Features.Product.Commands.DeleteProduct
{
    public class DeleteProductCommand : IRequest<bool>
    {
        public long Id { get; set; }
    }
}

