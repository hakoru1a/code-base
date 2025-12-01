using MediatR;

namespace Generate.Application.Features.Categories.Commands.DeleteCategory
{
    public class DeleteCategoryCommand : IRequest<bool>
    {
        public long Id { get; set; }
    }
}

