namespace Quom.AssetManagement.Web.Models;

public sealed class CreateSupplierRequest
{
    public string Name { get; set; } = string.Empty;

    public string? TaxId { get; set; }

    public string? ContactName { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }
}