namespace Shared.SeedWork
{
    /// <summary>
    /// Combines RequestParameter and PagingRequestParameters for comprehensive request handling
    /// </summary>
    public class PagedRequestParameter : PagingRequestParameters
    {
        public string OrderBy { get; set; } = string.Empty;
        public string OrderByDirection { get; set; } = "asc";
        public string SearchTerms { get; set; } = string.Empty;
    }
}

