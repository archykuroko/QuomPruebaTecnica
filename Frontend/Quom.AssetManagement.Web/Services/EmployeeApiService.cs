using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Quom.AssetManagement.Web.Models;
using Quom.AssetManagement.Web.State;

namespace Quom.AssetManagement.Web.Services
{
    public class EmployeeApiService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthState _authState;


        public EmployeeApiService(
            HttpClient httpClient,
            AuthState authState)
        {
            _httpClient = httpClient;
            _authState = authState;
        }


        // =====================================================
        // GET /api/employees
        // =====================================================

        public async Task<IEnumerable<EmployeeModel>> GetAllAsync()
        {
            using var request =
                CreateAuthorizedRequest(
                    HttpMethod.Get,
                    "api/employees");


            var response =
                await _httpClient.SendAsync(request);


            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                _authState.ClearSession();

                throw new UnauthorizedAccessException(
                    "La sesión ha expirado.");
            }


            response.EnsureSuccessStatusCode();


            return await response.Content
                .ReadFromJsonAsync<IEnumerable<EmployeeModel>>()
                ?? [];
        }


        // =====================================================
        // POST /api/employees
        // Sólo Administrador
        // =====================================================

        public async Task<int> CreateAsync(
            CreateEmployeeRequest request)
        {
            using var message =
                CreateAuthorizedRequest(
                    HttpMethod.Post,
                    "api/employees");


            message.Content =
                JsonContent.Create(request);


            var response =
                await _httpClient.SendAsync(message);


            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                _authState.ClearSession();

                throw new UnauthorizedAccessException(
                    "La sesión ha expirado.");
            }


            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new UnauthorizedAccessException(
                    "No tienes permisos para crear empleados.");
            }


            if (!response.IsSuccessStatusCode)
            {
                var error =
                    await response.Content
                        .ReadFromJsonAsync<ApiErrorResponse>();

                throw new InvalidOperationException(
                    error?.Message
                    ?? "No fue posible crear el empleado.");
            }


            var result =
                await response.Content
                    .ReadFromJsonAsync<CreateEmployeeResponse>();


            if (result is null)
            {
                throw new InvalidOperationException(
                    "La API no devolvió el identificador del empleado.");
            }


            return result.Id;
        }


        // =====================================================
        // REQUEST AUTORIZADO
        // =====================================================

        private HttpRequestMessage CreateAuthorizedRequest(
            HttpMethod method,
            string url)
        {
            var request =
                new HttpRequestMessage(
                    method,
                    url);


            if (!string.IsNullOrWhiteSpace(_authState.Token))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue(
                        "Bearer",
                        _authState.Token);
            }


            return request;
        }
    }
}