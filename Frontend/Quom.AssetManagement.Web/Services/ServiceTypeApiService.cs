using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Quom.AssetManagement.Web.Models;
using Quom.AssetManagement.Web.State;

namespace Quom.AssetManagement.Web.Services
{
    public class ServiceTypeApiService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthState _authState;


        public ServiceTypeApiService(
            HttpClient httpClient,
            AuthState authState)
        {
            _httpClient = httpClient;
            _authState = authState;
        }


        public async Task<IEnumerable<ServiceTypeModel>> GetAllAsync()
        {
            using var request =
                new HttpRequestMessage(
                    HttpMethod.Get,
                    "api/service-types");


            if (!string.IsNullOrWhiteSpace(_authState.Token))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue(
                        "Bearer",
                        _authState.Token);
            }


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
                .ReadFromJsonAsync<IEnumerable<ServiceTypeModel>>()
                ?? [];
        }
    }
}