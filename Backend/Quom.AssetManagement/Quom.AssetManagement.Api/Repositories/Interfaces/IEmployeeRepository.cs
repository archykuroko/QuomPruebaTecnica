using Quom.AssetManagement.Api.DTOs.Employees;
using Quom.AssetManagement.Api.Models;

namespace Quom.AssetManagement.Api.Repositories.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<IEnumerable<Employee>> GetAllAsync();
        Task<int> CreateAsync(CreateEmployeeRequest request);
    }
}