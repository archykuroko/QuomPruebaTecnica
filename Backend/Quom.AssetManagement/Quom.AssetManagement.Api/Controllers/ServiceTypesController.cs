using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quom.AssetManagement.Api.Services.Interfaces;

namespace Quom.AssetManagement.Api.Controllers
{
    [ApiController]
    [Route("api/service-types")]
    [Authorize]

    public class ServiceTypesController : ControllerBase
    {
        private readonly IServiceTypeService _serviceTypeService;

        public ServiceTypesController(IServiceTypeService serviceTypeService)
        {
            _serviceTypeService = serviceTypeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var serviceTypes = await _serviceTypeService.GetAllAsync();

            return Ok(serviceTypes);
        }
    }
}