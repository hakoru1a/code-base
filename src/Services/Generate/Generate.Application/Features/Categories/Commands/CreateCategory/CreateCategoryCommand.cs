using MediatR;

namespace Generate.Application.Features.Categories.Commands.CreateCategory
{
    public class CreateCategoryCommand : IRequest<long>
    {
        public string Name { get; set; } = string.Empty;
    }
}
