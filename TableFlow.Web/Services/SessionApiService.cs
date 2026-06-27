using System.Net;
using TableFlow.Web.Auth;
using TableFlow.Web.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace TableFlow.Web.Services;

public class SessionApiService
{
    private readonly HttpClient _http;
    private readonly CustomAuthStateProvider _authProvider;
    private readonly UnauthorizedNotifier _notifier;

    public SessionApiService(
        IHttpClientFactory httpClientFactory,
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

    // Used by client menu pages — no auth needed
    public async Task<bool> IsSessionActiveAsync(int sessionId)
    {
        try
        {
            var res = await _http.GetFromJsonAsync<SessionStatusResponse>(
                $"/api/sessions/{sessionId}/status");
            return res?.IsOpen ?? false;
        }
        catch
        {
            return false; // 404 or any error = treat as invalid
        }
    }

    private record SessionStatusResponse(bool IsOpen);

    // Resolves a static table QR token to its current session (anonymous — customer scan).
    // Returns null when the token doesn't match any table (invalid QR).
    public async Task<TableResolveModel?> ResolveTableTokenAsync(Guid token)
    {
        try
        {
            var res = await _http.GetAsync($"/api/sessions/by-table-token/{token}");
            if (!res.IsSuccessStatusCode) return null; // 404 = unknown table/token
            return await res.Content.ReadFromJsonAsync<TableResolveModel>();
        }
        catch
        {
            return null;
        }
    }

    public async Task<(bool Success, SessionModel? Session, string Error)> OpenSessionAsync(int tableId)
    {
        await AttachTokenAsync();
        var res = await _http.PostAsJsonAsync("/api/sessions", new { TableId = tableId });
        if (await CheckUnauthorizedAsync(res)) return (false, null, "Unauthorized");
        if (res.IsSuccessStatusCode)
        {
            var json = await res.Content.ReadAsStringAsync();
            var session = System.Text.Json.JsonSerializer.Deserialize<SessionModel>(json,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return (true, session, "");
        }
        var error = await res.Content.ReadAsStringAsync();
        return (false, null, error);
    }

    public async Task<SessionModel?> GetSessionByIdAsync(int sessionId)
    {
        await AttachTokenAsync();
        try
        {
            var res = await _http.GetAsync($"/api/sessions/{sessionId}");
            if (await CheckUnauthorizedAsync(res)) return null;
            if (!res.IsSuccessStatusCode) return null;
            return await res.Content.ReadFromJsonAsync<SessionModel>();
        }
        catch
        {
            return null;
        }
    }

    public async Task<SessionModel?> GetActiveSessionAsync(int tableId)
    {
        await AttachTokenAsync();
        try
        {
            var response = await _http.GetAsync($"/api/sessions/table/{tableId}/active");
            if (await CheckUnauthorizedAsync(response)) return null;
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<SessionModel>();
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<SessionPagedResult> GetSessionHistoryAsync(
        int page = 1, int pageSize = 25, string filter = "all", string? search = null)
    {
        await AttachTokenAsync();
        var url = $"/api/sessions?page={page}&pageSize={pageSize}&filter={filter}";
        if (!string.IsNullOrWhiteSpace(search))
            url += $"&search={Uri.EscapeDataString(search)}";

        var res = await _http.GetAsync(url);
        if (await CheckUnauthorizedAsync(res)) return new SessionPagedResult();
        if (!res.IsSuccessStatusCode) return new SessionPagedResult();
        return await res.Content.ReadFromJsonAsync<SessionPagedResult>(
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? new SessionPagedResult();
    }

    public async Task<CashierStatsModel?> GetMyStatsAsync()
    {
        await AttachTokenAsync();
        var res = await _http.GetAsync("/api/sessions/my-stats");
        if (await CheckUnauthorizedAsync(res)) return null;
        if (!res.IsSuccessStatusCode) return null;
        return await res.Content.ReadFromJsonAsync<CashierStatsModel>(
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<SessionStatsModel?> GetSessionStatsAsync()
    {
        await AttachTokenAsync();
        var res = await _http.GetAsync("/api/sessions/stats");
        if (await CheckUnauthorizedAsync(res)) return null;
        if (!res.IsSuccessStatusCode) return null;
        return await res.Content.ReadFromJsonAsync<SessionStatsModel>(
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<(bool Success, SessionModel? Session, string Error)> CloseSessionAsync(
        int sessionId, string paymentMethod, decimal amountReceived)
    {
        await AttachTokenAsync();
        var res = await _http.PatchAsJsonAsync($"/api/sessions/{sessionId}/close",
            new { PaymentMethod = paymentMethod, AmountReceived = amountReceived });
        if (await CheckUnauthorizedAsync(res)) return (false, null, "Unauthorized");
        if (res.IsSuccessStatusCode)
        {
            var session = await res.Content.ReadFromJsonAsync<SessionModel>();
            return (true, session, "");
        }
        var error = await res.Content.ReadAsStringAsync();
        return (false, null, error);
    }
}
