namespace Shared.SeedWork
{
    /// <summary>
    /// Represents a paginated result with metadata
    /// </summary>
    /// <typeparam name="T">Type of data being paginated</typeparam>
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public PaginationMetadata? Pagination { get; set; }

        public PagedResult(List<T> items, int count, int pageNumber, int pageSize)
        {
            Items = items;
            Pagination = new PaginationMetadata
            {
                TotalCount = count,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(count / (double)pageSize),
                HasPrevious = pageNumber > 1,
                HasNext = pageNumber < (int)Math.Ceiling(count / (double)pageSize)
            };
        }

        public static PagedResult<T> Create(List<T> items, int count, int pageNumber, int pageSize)
        {
            return new PagedResult<T>(items, count, pageNumber, pageSize);
        }

        /// <summary>
        /// Converts PagedResult to ApiSuccessResult with pagination metadata
        /// </summary>
        public ApiSuccessResult<List<T>> ToApiResult(string? message = null)
        {
            return ApiSuccessResult<List<T>>.WithPagination(
                Items,
                Pagination ?? new PaginationMetadata(),
                message ?? ResponseMessages.RetrieveItemsSuccess
            );
        }
    }

    /// <summary>
    /// Pagination metadata
    /// </summary>
    public class PaginationMetadata
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public bool HasPrevious { get; set; }
        public bool HasNext { get; set; }
        public int FirstRowOnPage => TotalCount > 0 ? (CurrentPage - 1) * PageSize + 1 : 0;
        public int LastRowOnPage => Math.Min(CurrentPage * PageSize, TotalCount);
    }
}

