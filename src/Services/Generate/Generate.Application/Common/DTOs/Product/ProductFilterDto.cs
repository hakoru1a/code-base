using Shared.DTOs;

namespace Generate.Application.Common.DTOs.Product
{
    /// <summary>
    /// DTO for filtering and querying Products with pagination
    /// </summary>
    public class ProductFilterDto : BaseFilterDto
    {
        // Specific filter properties for Product
        public string? Name { get; set; }
        public long? CategoryId { get; set; }
        
        // Additional filter properties
        public DateTimeOffset? CreatedFrom { get; set; }
        public DateTimeOffset? CreatedTo { get; set; }
    }
}

