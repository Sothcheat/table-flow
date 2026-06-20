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

    public async Task<List<Models.SessionModel>> GetSessionHistoryAsync()
    {
        await AttachTokenAsync();
        var res = await _http.GetAsync("/api/sessions");
        if (await CheckUnauthorizedAsync(res)) return new List<Models.SessionModel>();
        if (!res.IsSuccessStatusCode) return new List<Models.SessionModel>();
        return await res.Content.ReadFromJsonAsync<List<Models.SessionModel>>(
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? new List<Models.SessionModel>();
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

    public async Task<(bool Success, SessionModel? Session, string Error)> CloseSessionAsync(int sessionId, string paymentMethod)
    {
        await AttachTokenAsync();
        var res = await _http.PatchAsJsonAsync($"/api/sessions/{sessionId}/close", new { PaymentMethod = paymentMethod });
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
