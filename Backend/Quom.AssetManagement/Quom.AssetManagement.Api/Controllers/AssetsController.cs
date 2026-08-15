using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quom.AssetManagement.Api.DTOs.Assets;
using Quom.AssetManagement.Api.Services.Interfaces;
using System.Security.Claims;


namespace Quom.AssetManagement.Api.Controllers
{
    [ApiController]
    [Route("api/assets")]
    [Authorize]
    public class AssetsController : ControllerBase
    {
        private readonly IAssetService _assetService;

        public AssetsController(IAssetService assetService)
        {
            _assetService = assetService;
        }


        // Endpoint de búsqueda por ID
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(
            int id)
        {
            var asset = await _assetService.GetByIdAsync(id);

            if (asset is null)
                return NotFound();

            return Ok(asset);
        }

        // Endpoint de búsqueda por filtros
        [HttpGet]
        public async Task<IActionResult> Search(
            [FromQuery] AssetSearchRequest request)
        {
            var result = await _assetService.SearchAsync(request);

            return Ok(result);
        }


        // Endpoint para creación de activos
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public async Task<IActionResult> Create(
          [FromBody] CreateAssetRequest request)
        {
            var userId = GetCurrentUserId();

            var id = await _assetService.CreateAsync(
                request,
                userId);

            return CreatedAtAction(
                nameof(GetById),
                new { id },
                new { id });
        }

        // Endpoint para la actualización de activos
        [Authorize(Roles = "Administrador")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateAssetRequest request)
        {
            var userId = GetCurrentUserId();

            await _assetService.UpdateAsync(
                id,
                request,
                userId);

            return NoContent();
        }


        // Endpoint para asignación de activos
        [HttpPost("{id:int}/assign")]
        public async Task<IActionResult> Assign(
             int id,
             [FromBody] AssignAssetRequest request)
        {
            var userId = GetCurrentUserId();

            await _assetService.AssignAsync(
                id,
                request,
                userId);

            return NoContent();
        }

        // Endpoint para la devolución de un activo
        [HttpPost("{id:int}/return")]
        public async Task<IActionResult> Return(
              int id,
              [FromBody] ReturnAssetRequest request)
        {
            var userId = GetCurrentUserId();

            await _assetService.ReturnAsync(
                id,
                request,
                userId);

            return NoContent();
        }

        // Endpoint para regresar el historial de activos por ID
        [HttpGet("{id:int}/history")]
        public async Task<IActionResult> GetHistory(int id)
        {
            var history = await _assetService.GetHistoryAsync(id);

            return Ok(history);
        }


        // Helpers
        private int GetCurrentUserId()
        {
            var userIdClaim =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var userId))
                throw new UnauthorizedAccessException(
                    "No fue posible identificar al usuario autenticado.");

            return userId;
        }


    }
}