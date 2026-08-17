namespace Quom.AssetManagement.Web.Models
{
    public class AssetSearchRequest
    {
        public string? Search { get; set; }

        public string? Status { get; set; }

        public string? Category { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}