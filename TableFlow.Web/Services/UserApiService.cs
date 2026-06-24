using Microsoft.AspNetCore.Components.Authorization;
using System.Net;
using System.Net.Http.Headers;
using TableFlow.Web.Auth;
using TableFlow.Web.Models;

namespace TableFlow.Web.Services
{
    public class UserApiService
    {
        private readonly HttpClient _http;
        private readonly CustomAuthStateProvider _authProvider;
        private readonly UnauthorizedNotifier _notifier;

        public UserApiService(
            IHttpClientFactory httpClientFactory,
            AuthenticationStateProvider authProvider,
            UnauthorizedNotifier notifier)
        {
            _http = httpClientFactory.CreateClient("TableFlowApi");
            _authProvider = (CustomAuthStateProvider)authProvider;
            _notifier = notifier;
        }

        private async Task AuthorizeRequest()
        {
            var token = await _authProvider.GetTokenAsync();
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        private async Task<bool> CheckUnauthorizedAsync(HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await _notifier.NotifyAsync();
                return true;
            }
            return false;
        }

        public async Task<List<StaffUserModel>> GetUsersAsync()
        {
            await AuthorizeRequest();
            var response = await _http.GetAsync("/api/users");
            if (await CheckUnauthorizedAsync(response)) return new List<StaffUserModel>();
            if (!response.IsSuccessStatusCode) return new List<StaffUserModel>();
            return await response.Content.ReadFromJsonAsync<List<StaffUserModel>>() ?? new List<StaffUserModel>();
        }

        public async Task<bool> CreateUserAsync(CreateUserModel model)
        {
            await AuthorizeRequest();
            var response = await _http.PostAsJsonAsync("/api/users", model);
            if (await CheckUnauthorizedAsync(response)) return false;
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateUserAsync(string id, UpdateUserModel model)
        {
            await AuthorizeRequest();
            var response = await _http.PutAsJsonAsync($"/api/users/{id}", model);
            if (await CheckUnauthorizedAsync(response)) return false;
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            await AuthorizeRequest();
            var response = await _http.DeleteAsync($"/api/users/{id}");
            if (await CheckUnauthorizedAsync(response)) return false;
            return response.IsSuccessStatusCode;
        }
    }
}
