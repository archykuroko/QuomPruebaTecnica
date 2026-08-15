using System.ComponentModel.DataAnnotations;

namespace Quom.AssetManagement.Api.DTOs.Assets
{
    public class ReturnAssetRequest
    {
        [Required]
        [MaxLength(250)]
        public string ReturnCondition { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}