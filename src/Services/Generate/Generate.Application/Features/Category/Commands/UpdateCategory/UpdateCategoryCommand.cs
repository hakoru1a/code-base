using MediatR;

namespace Generate.Application.Features.Category.Commands.UpdateCategory
{
    public class UpdateCategoryCommand : IRequest<bool>
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}

