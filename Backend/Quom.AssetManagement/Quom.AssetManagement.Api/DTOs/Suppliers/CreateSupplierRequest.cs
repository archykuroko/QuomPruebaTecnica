using System.ComponentModel.DataAnnotations;

namespace Quom.AssetManagement.Api.DTOs.Suppliers
{
    public class CreateSupplierRequest
    {
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? TaxId { get; set; }

        [MaxLength(150)]
        public string? ContactName { get; set; }

        [EmailAddress]
        [MaxLength(150)]
        public string? Email { get; set; }

        [MaxLength(30)]
        public string? Phone { get; set; }
    }
}