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
}