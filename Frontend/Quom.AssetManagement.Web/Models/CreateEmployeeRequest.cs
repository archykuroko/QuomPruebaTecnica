namespace Quom.AssetManagement.Web.Models;

public sealed class CreateEmployeeRequest
{
    public string EmployeeNumber { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Department { get; set; }

    public string? Location { get; set; }
}