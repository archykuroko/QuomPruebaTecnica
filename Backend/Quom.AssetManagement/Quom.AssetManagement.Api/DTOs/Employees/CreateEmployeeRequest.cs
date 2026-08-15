using System.ComponentModel.DataAnnotations;

namespace Quom.AssetManagement.Api.DTOs.Employees
{
    public class CreateEmployeeRequest
    {
        [Required]
        [MaxLength(50)]
        public string EmployeeNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Department { get; set; }

        [MaxLength(150)]
        public string? Location { get; set; }
    }
}