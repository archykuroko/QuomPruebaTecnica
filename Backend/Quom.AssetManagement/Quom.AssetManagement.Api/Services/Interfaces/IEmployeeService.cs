using Quom.AssetManagement.Api.DTOs.Employees;
using Quom.AssetManagement.Api.Models;

namespace Quom.AssetManagement.Api.Services.Interfaces
{
    public interface IEmployeeService
    {
        Task<IEnumerable<Employee>> GetAllAsync();
        Task<int> CreateAsync(CreateEmployeeRequest request);
    }
}