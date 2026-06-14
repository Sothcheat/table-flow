using TableFlow.Web.Auth;
using Microsoft.AspNetCore.Components.Authorization;

namespace TableFlow.Web.Services;

public class SessionApiService
{
    private readonly HttpClient _http;
    private readonly CustomAuthStateProvider _authProvider;

    public SessionApiService(
        IHttpClientFactory httpClientFactory,
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
            return await _http.GetFromJsonAsync<SessionModel>($"/api/sessions/{sessionId}");
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
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<SessionModel>();

            var error = await response.Content.ReadAsStringAsync();
            return null;
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    public async Task<(bool Success, SessionModel? Session, string Error)> CloseSessionAsync(int sessionId, string paymentMethod)
    {
        await AttachTokenAsync();
        var res = await _http.PatchAsJsonAsync($"/api/sessions/{sessionId}/close", new { PaymentMethod = paymentMethod });
        if (res.IsSuccessStatusCode)
        {
            var session = await res.Content.ReadFromJsonAsync<SessionModel>();
            return (true, session, "");
        }
        var error = await res.Content.ReadAsStringAsync();
        return (false, null, error);
    }
}
