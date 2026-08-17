namespace Quom.AssetManagement.Web.Models
{
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = [];

        public int TotalRecords { get; set; }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public int TotalPages =>
            PageSize <= 0
                ? 0
                : (int)Math.Ceiling(
                    TotalRecords / (double)PageSize);
    }
}