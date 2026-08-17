using System.Net.Http.Headers;
using System.Net.Http.Json;
using Quom.AssetManagement.Web.Models;
using Quom.AssetManagement.Web.State;

namespace Quom.AssetManagement.Web.Services
{
    public class AssetApiService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthState _authState;

        public AssetApiService(
            HttpClient httpClient,
            AuthState authState)
        {
            _httpClient = httpClient;
            _authState = authState;
        }


        public async Task<PagedResult<AssetModel>?> SearchAsync(
            AssetSearchRequest request)
        {
            var query = new List<string>();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query.Add(
                    $"Search={Uri.EscapeDataString(request.Search)}");
            }

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                query.Add(
                    $"Status={Uri.EscapeDataString(request.Status)}");
            }

            if (!string.IsNullOrWhiteSpace(request.Category))
            {
                query.Add(
                    $"Category={Uri.EscapeDataString(request.Category)}");
            }

            query.Add($"PageNumber={request.PageNumber}");
            query.Add($"PageSize={request.PageSize}");

            var url =
                $"api/assets?{string.Join("&", query)}";

            using var message =
                CreateAuthorizedRequest(
                    HttpMethod.Get,
                    url);

            var response =
                await _httpClient.SendAsync(message);

            if (response.StatusCode ==
                System.Net.HttpStatusCode.Unauthorized)
            {
                _authState.ClearSession();

                return null;
            }

            response.EnsureSuccessStatusCode();

            return await response.Content
                .ReadFromJsonAsync<PagedResult<AssetModel>>();
        }


        public async Task<AssetModel?> GetByIdAsync(int id)
        {
            using var message =
                CreateAuthorizedRequest(
                    HttpMethod.Get,
                    $"api/assets/{id}");

            var response =
                await _httpClient.SendAsync(message);

            if (response.StatusCode ==
                System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            return await response.Content
                .ReadFromJsonAsync<AssetModel>();
        }


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

        public async Task<IEnumerable<AssetMovementModel>> GetHistoryAsync(
          int assetId)
        {
            using var message =
                CreateAuthorizedRequest(
                    HttpMethod.Get,
                    $"api/assets/{assetId}/history");

            var response =
                await _httpClient.SendAsync(message);

            if (response.StatusCode ==
                System.Net.HttpStatusCode.Unauthorized)
            {
                _authState.ClearSession();

                return [];
            }

            response.EnsureSuccessStatusCode();

            return await response.Content
                .ReadFromJsonAsync<IEnumerable<AssetMovementModel>>()
                ?? [];
        }

        public async Task AssignAsync(
            int assetId,
            AssignAssetRequest request)
        {
            using var message =
                CreateAuthorizedRequest(
                    HttpMethod.Post,
                    $"api/assets/{assetId}/assign");

            message.Content =
                JsonContent.Create(request);

            var response =
                await _httpClient.SendAsync(message);

            if (response.StatusCode ==
                System.Net.HttpStatusCode.Unauthorized)
            {
                _authState.ClearSession();

                throw new UnauthorizedAccessException(
                    "La sesión ha expirado.");
            }

            if (!response.IsSuccessStatusCode)
            {
                var error =
                    await response.Content
                        .ReadFromJsonAsync<ApiErrorResponse>();

                throw new InvalidOperationException(
                    error?.Message
                    ?? "No fue posible asignar el activo.");
            }
        }


        public async Task ReturnAsync(
            int assetId,
            ReturnAssetRequest request)
        {
            using var message =
                CreateAuthorizedRequest(
                    HttpMethod.Post,
                    $"api/assets/{assetId}/return");

            message.Content =
                JsonContent.Create(request);

            var response =
                await _httpClient.SendAsync(message);

            if (response.StatusCode ==
                System.Net.HttpStatusCode.Unauthorized)
            {
                _authState.ClearSession();

                throw new UnauthorizedAccessException(
                    "La sesión ha expirado.");
            }

            if (!response.IsSuccessStatusCode)
            {
                var error =
                    await response.Content
                        .ReadFromJsonAsync<ApiErrorResponse>();

                throw new InvalidOperationException(
                    error?.Message
                    ?? "No fue posible registrar la devolución.");
            }
        }


        public async Task<int> CreateAsync(
            CreateAssetRequest request)
        {
            using var message =
                CreateAuthorizedRequest(
                    HttpMethod.Post,
                    "api/assets");

            message.Content =
                JsonContent.Create(request);

            var response =
                await _httpClient.SendAsync(message);

            if (response.StatusCode ==
                System.Net.HttpStatusCode.Unauthorized)
            {
                _authState.ClearSession();

                throw new UnauthorizedAccessException(
                    "La sesión ha expirado.");
            }

            if (response.StatusCode ==
                System.Net.HttpStatusCode.Forbidden)
            {
                throw new UnauthorizedAccessException(
                    "No tienes permisos para crear activos.");
            }

            if (!response.IsSuccessStatusCode)
            {
                var error =
                    await response.Content
                        .ReadFromJsonAsync<ApiErrorResponse>();

                throw new InvalidOperationException(
                    error?.Message
                    ?? "No fue posible crear el activo.");
            }

            var result =
                await response.Content
                    .ReadFromJsonAsync<CreateAssetResponse>();

            return result?.Id
                ?? throw new InvalidOperationException(
                    "El servidor no devolvió el identificador del activo.");
        }


        public async Task UpdateAsync(
            int assetId,
            UpdateAssetRequest request)
        {
            using var message =
                CreateAuthorizedRequest(
                    HttpMethod.Put,
                    $"api/assets/{assetId}");

            message.Content =
                JsonContent.Create(request);

            var response =
                await _httpClient.SendAsync(message);

            if (response.StatusCode ==
                System.Net.HttpStatusCode.Unauthorized)
            {
                _authState.ClearSession();

                throw new UnauthorizedAccessException(
                    "La sesión ha expirado.");
            }

            if (response.StatusCode ==
                System.Net.HttpStatusCode.Forbidden)
            {
                throw new UnauthorizedAccessException(
                    "No tienes permisos para editar activos.");
            }

            if (!response.IsSuccessStatusCode)
            {
                var error =
                    await response.Content
                        .ReadFromJsonAsync<ApiErrorResponse>();

                throw new InvalidOperationException(
                    error?.Message
                    ?? "No fue posible actualizar el activo.");
            }
        }



    }
}