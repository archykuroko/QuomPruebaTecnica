using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quom.AssetManagement.Api.DTOs.Employees;
using Quom.AssetManagement.Api.Services.Interfaces;

namespace Quom.AssetManagement.Api.Controllers
{
    [ApiController]
    [Route("api/employees")]
    [Authorize]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeesController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        // Endpoint para obtener a todos los empleados
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var employees = await _employeeService.GetAllAsync();

            return Ok(employees);
        }

        // Endpoint para crear un nuevo empleado
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateEmployeeRequest request)
        {
            var id = await _employeeService.CreateAsync(request);

            return StatusCode(
                StatusCodes.Status201Created,
                new { id });
        }
    }
}