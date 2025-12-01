using MediatR;

namespace Generate.Application.Features.Products.Commands.DeleteProduct
{
    public class DeleteProductCommand : IRequest<bool>
    {
        public long Id { get; set; }
    }
}

