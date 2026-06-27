using System.Net;
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
        private readonly UnauthorizedNotifier _notifier;

        public TableApiService(IHttpClientFactory httpClientFactory,
            AuthenticationStateProvider authProvider,
            UnauthorizedNotifier notifier)
        {
            _http = httpClientFactory.CreateClient("TableFlowApi");
            _authProvider = (CustomAuthStateProvider)authProvider;
            _notifier = notifier;
        }

        private async Task AttachTokenAsync()
        {
            var token = await _authProvider.GetTokenAsync();
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
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

        public async Task<List<TableModel>> GetTablesAsync()
        {
            await AttachTokenAsync();
            var response = await _http.GetAsync("/api/tables");
            if (await CheckUnauthorizedAsync(response)) return [];
            if (!response.IsSuccessStatusCode) return [];
            return await response.Content.ReadFromJsonAsync<List<TableModel>>() ?? [];
        }

        public async Task<TableQrModel?> GetTableQrAsync(int tableId)
        {
            await AttachTokenAsync();
            var response = await _http.GetAsync($"/api/tables/{tableId}/qr");
            if (await CheckUnauthorizedAsync(response)) return null;
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<TableQrModel>();
        }

        public async Task<(bool Success, string Error)> CreateTableAsync(int tableNumber)
        {
            await AttachTokenAsync();
            var response = await _http.PostAsJsonAsync("/api/tables", new { TableNumber = tableNumber });
            if (await CheckUnauthorizedAsync(response)) return (false, "Unauthorized");
            if (response.IsSuccessStatusCode) return (true, "");
            var error = await response.Content.ReadAsStringAsync();
            return (false, error);
        }

        public async Task<(bool Success, string Error)> UpdateTableAsync(int id, int tableNumber)
        {
            await AttachTokenAsync();
            var response = await _http.PutAsJsonAsync($"/api/tables/{id}", new { TableNumber = tableNumber });
            if (await CheckUnauthorizedAsync(response)) return (false, "Unauthorized");
            if (response.IsSuccessStatusCode) return (true, "");
            var error = await response.Content.ReadAsStringAsync();
            return (false, error);
        }

        public async Task<(bool Success, string Error)> DeleteTableAsync(int id)
        {
            await AttachTokenAsync();
            var response = await _http.DeleteAsync($"/api/tables/{id}");
            if (await CheckUnauthorizedAsync(response)) return (false, "Unauthorized");
            if (response.IsSuccessStatusCode) return (true, "");
            var error = await response.Content.ReadAsStringAsync();
            return (false, error);
        }

        public async Task<(bool Success, string Error)> UpdateTableStatusAsync(int id, string status)
        {
            await AttachTokenAsync();
            var response = await _http.PatchAsJsonAsync($"/api/tables/{id}/status", new { Status = status });
            if (await CheckUnauthorizedAsync(response)) return (false, "Unauthorized");
            if (response.IsSuccessStatusCode) return (true, "");
            var error = await response.Content.ReadAsStringAsync();
            return (false, error);
        }
    }
}
