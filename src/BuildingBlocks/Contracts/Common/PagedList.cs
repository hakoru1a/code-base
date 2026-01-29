namespace Contracts.Common
{
    public class PagedList<T> : List<T>
    {
        private readonly MetaData _metaData;

        public PagedList(IEnumerable<T> items, long totalItems, int pageNumber, int pageSize)
        {
            _metaData = new MetaData
            {
                TotalItems = totalItems,
                PageSize = pageSize,
                CurrentPage = pageNumber,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            };

            AddRange(items);
        }

        public MetaData GetMetaData()
        {
            return _metaData;
        }
    }
}
