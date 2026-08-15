using System.ComponentModel.DataAnnotations;

namespace Quom.AssetManagement.Api.DTOs.Assets
{
    public class AssetSearchRequest
    {
        [MaxLength(100)]
        public string? Search { get; set; }

        [MaxLength(20)]
        public string? Status { get; set; }

        [MaxLength(50)]
        public string? Category { get; set; }

        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 10;
    }
}