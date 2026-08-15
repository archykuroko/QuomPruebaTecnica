using Microsoft.AspNetCore.Mvc;
using Quom.AssetManagement.Api.DTOs.Auth;
using Quom.AssetManagement.Api.Services.Interfaces;

namespace Quom.AssetManagement.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // Endpoint para iniciar sesión
        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);

            if (result is null)
            {
                return Unauthorized(new
                {
                    message = "Usuario o contraseña incorrectos."
                });
            }

            return Ok(result);
        }
    }
}