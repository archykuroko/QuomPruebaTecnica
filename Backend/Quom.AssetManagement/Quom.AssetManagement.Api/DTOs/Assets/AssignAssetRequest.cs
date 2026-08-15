using System.ComponentModel.DataAnnotations;

namespace Quom.AssetManagement.Api.DTOs.Assets
{
    public class AssignAssetRequest
    {
        [Range(1, int.MaxValue)]
        public int EmployeeId { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}