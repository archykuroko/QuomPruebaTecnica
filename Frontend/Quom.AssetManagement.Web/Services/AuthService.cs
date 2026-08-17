using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Quom.AssetManagement.Web.Models;
using Quom.AssetManagement.Web.State;

namespace Quom.AssetManagement.Web.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthState _authState;
        private readonly ProtectedSessionStorage _sessionStorage;

        public AuthService(
            HttpClient httpClient,
            AuthState authState,
            ProtectedSessionStorage sessionStorage)
        {
            _httpClient = httpClient;
            _authState = authState;
            _sessionStorage = sessionStorage;
        }

        public async Task<bool> LoginAsync(
            string login,
            string password)
        {
            var request = new LoginRequest
            {
                Login = login,
                Password = password
            };

            var response = await _httpClient.PostAsJsonAsync(
                "api/auth/login",
                request);

            if (!response.IsSuccessStatusCode)
                return false;

            var result =
                await response.Content
                    .ReadFromJsonAsync<LoginResponse>();

            if (result is null ||
                string.IsNullOrWhiteSpace(result.Token))
            {
                return false;
            }


            _authState.SetSession(result);


            await _sessionStorage.SetAsync(
                "quom-auth",
                new StoredAuthSession
                {
                    Token = result.Token,
                    UserId = result.UserId,
                    Username = result.Username,
                    Role = result.Role,
                    ExpiresAt = result.ExpiresAt
                });

            return true;
        }

        public async Task<bool> RestoreSessionAsync()
        {
            try
            {
                var stored =
                    await _sessionStorage
                        .GetAsync<StoredAuthSession>(
                            "quom-auth");

                if (!stored.Success ||
                    stored.Value is null)
                {
                    return false;
                }

                // Si el token ya expiró, limpiamos la sesión
                if (stored.Value.ExpiresAt <= DateTime.UtcNow)
                {
                    await LogoutAsync();

                    return false;
                }

                _authState.RestoreSession(
                    stored.Value);

                return true;
            }
            catch
            {
                // Si sessionStorage no está disponible o
                // existe algún problema recuperando la sesión,
                // simplemente consideramos que no hay sesión.
                return false;
            }
        }

        public async Task LogoutAsync()
        {
            _authState.ClearSession();

            try
            {
                await _sessionStorage.DeleteAsync(
                    "quom-auth");
            }
            catch
            {
                // El estado en memoria ya fue eliminado.
                // Evitamos romper el logout si el storage
                // del navegador no estuviera disponible.
            }
        }
    }
}