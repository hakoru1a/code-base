using Shared.SeedWork;

namespace Shared.DTOs
{
    /// <summary>
    /// Base class for filter/query DTOs with pagination support
    /// </summary>
    public abstract class BaseFilterDto : PagedRequestParameter
    {
        // Inherits PageNumber, PageSize, OrderBy, OrderByDirection, SearchTerms
        // Add any common filter properties here
    }
}

