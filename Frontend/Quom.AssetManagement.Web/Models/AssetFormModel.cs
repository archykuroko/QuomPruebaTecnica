namespace Quom.AssetManagement.Web.Models;

public sealed class AssetFormModel
{
    public int Id { get; set; }

    public string AssetCode { get; set; } = string.Empty;

    public string SerialNumber { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string Brand { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public string OwnershipType { get; set; } = "Propio";

    public string Status { get; set; } = "Disponible";

    public string CurrentLocation { get; set; } = string.Empty;

    public int? SupplierId { get; set; }

    public DateTime? PurchaseDate { get; set; }

    public DateTime? RentalEndDate { get; set; }
}

public sealed class SupplierOption
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}