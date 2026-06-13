using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Authorization;
using TableFlow.Web.Auth;
using TableFlow.Web.Models;

namespace TableFlow.Web.Services
{
    public class TableApiService
    {
        private readonly HttpClient _http;
        private readonly CustomAuthStateProvider _authProvider;

        public TableApiService(IHttpClientFactory httpClientFactory,
            AuthenticationStateProvider authProvider)
        {
            _http = httpClientFactory.CreateClient("TableFlowApi");
            _authProvider = (CustomAuthStateProvider)authProvider;
        }

        private async Task AttachTokenAsync()
        {
            var token = await _authProvider.GetTokenAsync();
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<List<TableModel>> GetTablesAsync()
        {
            await AttachTokenAsync();
            return await _http.GetFromJsonAsync<List<TableModel>>("/api/tables") ?? [];
        }

        public async Task<(bool Success, string Error)> CreateTableAsync(int tableNumber)
        {
            await AttachTokenAsync();
            var response = await _http.PostAsJsonAsync("/api/tables", new { TableNumber = tableNumber });
            if (response.IsSuccessStatusCode) return (true, "");
            var error = await response.Content.ReadAsStringAsync();
            return (false, error);
        }

        public async Task<(bool Success, string Error)> UpdateTableAsync(int id, int tableNumber)
        {
            await AttachTokenAsync();
            var response = await _http.PutAsJsonAsync($"/api/tables/{id}", new { TableNumber = tableNumber });
            if (response.IsSuccessStatusCode) return (true, "");
            var error = await response.Content.ReadAsStringAsync();
            return (false, error);
        }

        public async Task<(bool Success, string Error)> DeleteTableAsync(int id)
        {
            await AttachTokenAsync();
            var response = await _http.DeleteAsync($"/api/tables/{id}");
            if (response.IsSuccessStatusCode) return (true, "");
            var error = await response.Content.ReadAsStringAsync();
            return (false, error);
        }

        public async Task<(bool Success, string Error)> UpdateTableStatusAsync(int id, string status)
        {
            await AttachTokenAsync();
            var response = await _http.PatchAsJsonAsync($"/api/tables/{id}/status", new { Status = status });
            if (response.IsSuccessStatusCode) return (true, "");
            var error = await response.Content.ReadAsStringAsync();
            return (false, error);
        }
    }
}