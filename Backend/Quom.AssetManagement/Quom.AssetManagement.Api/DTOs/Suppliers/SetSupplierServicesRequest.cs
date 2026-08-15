using System.ComponentModel.DataAnnotations;

namespace Quom.AssetManagement.Api.DTOs.Suppliers
{
    public class SetSupplierServicesRequest
    {
        [Required]
        [MinLength(1)]
        public List<int> ServiceTypeIds { get; set; } = [];
    }
}