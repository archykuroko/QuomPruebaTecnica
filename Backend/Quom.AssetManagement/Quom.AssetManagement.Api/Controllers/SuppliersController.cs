using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quom.AssetManagement.Api.DTOs.Suppliers;
using Quom.AssetManagement.Api.Services.Interfaces;

namespace Quom.AssetManagement.Api.Controllers
{
    [ApiController]
    [Route("api/suppliers")]
    [Authorize]
    public class SuppliersController : ControllerBase
    {
        private readonly ISupplierService _supplierService;

        public SuppliersController(ISupplierService supplierService)
        {
            _supplierService = supplierService;
        }

        // Endpoint para obtener todos los proveedores
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var suppliers = await _supplierService.GetAllAsync();

            return Ok(suppliers);
        }

        // Endpoint para crear proveedores
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateSupplierRequest request)
        {
            var id = await _supplierService.CreateAsync(request);

            return StatusCode(
                StatusCodes.Status201Created,
                new { id });
        }

        // Endpoint para obtener los servicios de un proveedor
        [HttpGet("{id:int}/services")]
        public async Task<IActionResult> GetServices(int id)
        {
            var services = await _supplierService.GetServicesAsync(id);

            return Ok(services);
        }

        // Endpoint para establecer los servicios de un proveedor
        [Authorize(Roles = "Administrador")]
        [HttpPut("{id:int}/services")]
        public async Task<IActionResult> SetServices(
            int id,
            [FromBody] SetSupplierServicesRequest request)
        {
            await _supplierService.SetServicesAsync(id, request);

            return NoContent();
        }
    }
}