using Shared.DTOs;

namespace Generate.Application.Common.DTOs.Order
{
    /// <summary>
    /// DTO for filtering and querying Orders with pagination
    /// </summary>
    public class OrderFilterDto : BaseFilterDto
    {
        // Specific filter properties for Order
        public string? CustomerName { get; set; }
        
        // Additional filter properties
        public DateTimeOffset? CreatedFrom { get; set; }
        public DateTimeOffset? CreatedTo { get; set; }
        
        // Filter by minimum/maximum total items
        public int? MinItemCount { get; set; }
        public int? MaxItemCount { get; set; }
    }
}

