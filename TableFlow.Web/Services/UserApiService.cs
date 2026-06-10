using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using TableFlow.Web.Auth;
using TableFlow.Web.Models;

namespace TableFlow.Web.Services
{
    public class UserApiService
    {
        private readonly HttpClient _http;
        private readonly CustomAuthStateProvider _authProvider;

        public UserApiService(
            IHttpClientFactory httpClientFactory,
            AuthenticationStateProvider authProvider)
        {
            _http = httpClientFactory.CreateClient("TableFlowApi");
            _authProvider = (CustomAuthStateProvider)authProvider;
        }

        private async Task AuthorizeRequest()
        {
            var token = await _authProvider.GetTokenAsync();
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<List<StaffUserModel>> GetUsersAsync()
        {
            await AuthorizeRequest();
            var result = await _http.GetFromJsonAsync<List<StaffUserModel>>("/api/users");
            return result ?? new List<StaffUserModel>();
        }

        public async Task<bool> CreateUserAsync(CreateUserModel model)
        {
            await AuthorizeRequest();
            var response = await _http.PostAsJsonAsync("/api/users", model);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateUserAsync(string id, UpdateUserModel model)
        {
            await AuthorizeRequest();
            var response = await _http.PutAsJsonAsync($"/api/users/{id}", model);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            await AuthorizeRequest();
            var response = await _http.DeleteAsync($"/api/users/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
