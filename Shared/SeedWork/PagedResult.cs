namespace Shared.SeedWork
{
    /// <summary>
    /// Represents a paginated result with metadata
    /// </summary>
    /// <typeparam name="T">Type of data being paginated</typeparam>
    public class PagedResult<T>
    {
        public List<T> Data { get; set; } = new List<T>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;

        public PagedResult(List<T> data, int count, int pageNumber, int pageSize)
        {
            Data = data;
            TotalCount = count;
            CurrentPage = pageNumber;
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        }

        public static PagedResult<T> Create(List<T> data, int count, int pageNumber, int pageSize)
        {
            return new PagedResult<T>(data, count, pageNumber, pageSize);
        }
    }
}

