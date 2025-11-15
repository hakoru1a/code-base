using MediatR;
using Shared.DTOs.Product;

namespace Product.Application.Features.AttributeDefs.Commands.CreateAttribute
{
    public class CreateAttributeCommand : IRequest<AttributeDefDto>
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int Type { get; set; }
        public string? Options { get; set; }
        public bool IsRequired { get; set; }
        public int DisplayOrder { get; set; }
    }
}