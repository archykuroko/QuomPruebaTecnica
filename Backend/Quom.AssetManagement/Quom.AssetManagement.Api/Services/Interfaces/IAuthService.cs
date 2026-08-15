using Quom.AssetManagement.Api.DTOs.Auth;

namespace Quom.AssetManagement.Api.Services.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse?> LoginAsync(LoginRequest request);
    }
}