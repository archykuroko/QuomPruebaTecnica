namespace Quom.AssetManagement.Web.Models;

public sealed class UpdateAssetRequest
{
    public string AssetCode { get; set; } = string.Empty;

    public string? SerialNumber { get; set; }

    public string Category { get; set; } = string.Empty;

    public string Brand { get; set; } = string.Empty;

    public string? Model { get; set; }

    public string OwnershipType { get; set; } = string.Empty;

    public int? SupplierId { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? CurrentLocation { get; set; }

    public DateTime? PurchaseDate { get; set; }

    public DateTime? RentalEndDate { get; set; }
}