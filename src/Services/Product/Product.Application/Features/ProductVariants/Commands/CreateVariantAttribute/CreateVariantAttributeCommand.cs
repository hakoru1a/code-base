using MediatR;
using Shared.DTOs.Product;
using System.ComponentModel.DataAnnotations;

namespace Product.Application.Features.ProductVariants.Commands.CreateVariantAttribute
{
    /// <summary>
    /// Command for creating variant attribute
    /// </summary>
    public class CreateVariantAttributeCommand : IRequest<ProductVariantAttributeDto>
    {
        /// <summary>
        /// Product variant ID
        /// </summary>
        [Required]
        public long ProductVariantId { get; set; }

        /// <summary>
        /// Attribute definition ID
        /// </summary>
        [Required]
        public long AttributeDefId { get; set; }

        /// <summary>
        /// Attribute value
        /// </summary>
        [Required]
        [StringLength(500, ErrorMessage = "Value cannot exceed 500 characters")]
        public string Value { get; set; } = string.Empty;
    }
}

