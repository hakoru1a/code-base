using MediatR;

namespace Generate.Application.Features.Category.Commands.DeleteCategory
{
    public class DeleteCategoryCommand : IRequest<bool>
    {
        public long Id { get; set; }
    }
}

