namespace Quom.AssetManagement.Web.Models
{
    public class EmployeeModel
    {
        public int Id { get; set; }

        public string EmployeeNumber { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? Department { get; set; }

        public string? Location { get; set; }

        public bool IsActive { get; set; }

        public string FullName =>
            $"{FirstName} {LastName}".Trim();
    }
}