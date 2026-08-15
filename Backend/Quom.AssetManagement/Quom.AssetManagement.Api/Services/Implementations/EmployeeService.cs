using Quom.AssetManagement.Api.DTOs.Employees;
using Quom.AssetManagement.Api.Models;
using Quom.AssetManagement.Api.Repositories.Interfaces;
using Quom.AssetManagement.Api.Services.Interfaces;

namespace Quom.AssetManagement.Api.Services.Implementations
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;

        public EmployeeService(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public Task<IEnumerable<Employee>> GetAllAsync()
        {
            return _employeeRepository.GetAllAsync();
        }

        public Task<int> CreateAsync(CreateEmployeeRequest request)
        {
            return _employeeRepository.CreateAsync(request);
        }
    }
}