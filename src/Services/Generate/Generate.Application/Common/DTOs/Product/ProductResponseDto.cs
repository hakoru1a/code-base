using Shared.DTOs;

namespace Generate.Application.Common.DTOs.Product
{
    /// <summary>
    /// DTO for Product response
    /// </summary>
    public class ProductResponseDto : BaseResponseDto<long>
    {
        public string Name { get; set; } = string.Empty;
        public long? CategoryId { get; set; }
        public string? CategoryName { get; set; }
    }
}

