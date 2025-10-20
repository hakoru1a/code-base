using Shared.DTOs;

namespace Generate.Application.Common.DTOs.Category
{
    /// <summary>
    /// DTO for filtering and querying Categories with pagination
    /// </summary>
    public class CategoryFilterDto : BaseFilterDto
    {
        // Specific filter properties for Category
        public string? Name { get; set; }

        // Additional filter properties
        public DateTimeOffset? CreatedFrom { get; set; }
        public DateTimeOffset? CreatedTo { get; set; }

        // Example: Filter by minimum number of products
        // public int? MinProductCount { get; set; }
    }
}

