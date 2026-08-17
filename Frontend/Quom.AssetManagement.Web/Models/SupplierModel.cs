namespace Quom.AssetManagement.Web.Models
{
    public class SupplierModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? TaxId { get; set; }

        public string? ContactName { get; set; }

        public string? Email { get; set; }

        public string? Phone { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}