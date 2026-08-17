using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Quom.AssetManagement.Web.Models;
using Quom.AssetManagement.Web.State;

namespace Quom.AssetManagement.Web.Services
{
    public class SupplierApiService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthState _authState;


        public SupplierApiService(
            HttpClient httpClient,
            AuthState authState)
        {
            _httpClient = httpClient;
            _authState = authState;
        }


        // =====================================================
        // GET /api/suppliers
        // =====================================================

        public async Task<IEnumerable<SupplierModel>> GetAllAsync()
        {
            using var request =
                CreateAuthorizedRequest(
                    HttpMethod.Get,
                    "api/suppliers");


            var response =
                await _httpClient.SendAsync(request);


            HandleAuthentication(response);


            response.EnsureSuccessStatusCode();


            return await response.Content
                .ReadFromJsonAsync<IEnumerable<SupplierModel>>()
                ?? [];
        }


        // =====================================================
        // POST /api/suppliers
        // Sólo Administrador
        // =====================================================

        public async Task<int> CreateAsync(
            CreateSupplierRequest request)
        {
            using var message =
                CreateAuthorizedRequest(
                    HttpMethod.Post,
                    "api/suppliers");


            message.Content =
                JsonContent.Create(request);


            var response =
                await _httpClient.SendAsync(message);


            HandleAuthentication(response);


            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new UnauthorizedAccessException(
                    "No tienes permisos para crear proveedores.");
            }


            if (!response.IsSuccessStatusCode)
            {
                var error =
                    await TryReadErrorAsync(response);

                throw new InvalidOperationException(
                    error
                    ?? "No fue posible crear el proveedor.");
            }


            var result =
                await response.Content
                    .ReadFromJsonAsync<CreateSupplierResponse>();


            return result?.Id
                ?? throw new InvalidOperationException(
                    "La API no devolvió el identificador del proveedor.");
        }


        // =====================================================
        // GET /api/suppliers/{id}/services
        // =====================================================

        public async Task<IEnumerable<ServiceTypeModel>> GetServicesAsync(
            int supplierId)
        {
            using var request =
                CreateAuthorizedRequest(
                    HttpMethod.Get,
                    $"api/suppliers/{supplierId}/services");


            var response =
                await _httpClient.SendAsync(request);


            HandleAuthentication(response);


            response.EnsureSuccessStatusCode();


            return await response.Content
                .ReadFromJsonAsync<IEnumerable<ServiceTypeModel>>()
                ?? [];
        }


        // =====================================================
        // PUT /api/suppliers/{id}/services
        // Sólo Administrador
        // =====================================================

        public async Task SetServicesAsync(
            int supplierId,
            SetSupplierServicesRequest request)
        {
            using var message =
                CreateAuthorizedRequest(
                    HttpMethod.Put,
                    $"api/suppliers/{supplierId}/services");


            message.Content =
                JsonContent.Create(request);


            var response =
                await _httpClient.SendAsync(message);


            HandleAuthentication(response);


            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new UnauthorizedAccessException(
                    "No tienes permisos para modificar los servicios del proveedor.");
            }


            if (!response.IsSuccessStatusCode)
            {
                var error =
                    await TryReadErrorAsync(response);

                throw new InvalidOperationException(
                    error
                    ?? "No fue posible actualizar los servicios del proveedor.");
            }
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


        // =====================================================
        // AUTH
        // =====================================================

        private void HandleAuthentication(
            HttpResponseMessage response)
        {
            if (response.StatusCode != HttpStatusCode.Unauthorized)
                return;


            _authState.ClearSession();


            throw new UnauthorizedAccessException(
                "La sesión ha expirado.");
        }


        // =====================================================
        // ERROR
        // =====================================================

        private static async Task<string?> TryReadErrorAsync(
            HttpResponseMessage response)
        {
            try
            {
                var error =
                    await response.Content
                        .ReadFromJsonAsync<ApiErrorResponse>();

                return error?.Message;
            }
            catch
            {
                return null;
            }
        }
    }
}