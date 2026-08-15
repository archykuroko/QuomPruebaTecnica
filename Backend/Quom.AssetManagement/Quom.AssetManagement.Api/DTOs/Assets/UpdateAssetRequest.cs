using System.ComponentModel.DataAnnotations;

namespace Quom.AssetManagement.Api.DTOs.Assets
{
    public class UpdateAssetRequest
    {
        [Required]
        [MaxLength(50)]
        public string AssetCode { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? SerialNumber { get; set; }

        [Required]
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Brand { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Model { get; set; }

        [Required]
        [MaxLength(20)]
        public string OwnershipType { get; set; } = string.Empty;

        public int? SupplierId { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? CurrentLocation { get; set; }

        public DateTime? PurchaseDate { get; set; }

        public DateTime? RentalEndDate { get; set; }
        

    }
}