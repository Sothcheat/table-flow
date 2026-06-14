using Microsoft.AspNetCore.Components.Authorization;
using TableFlow.Web.Auth;
using TableFlow.Web.Models;

namespace TableFlow.Web.Services;

public class OrderApiService
{
    private readonly HttpClient _http;
    private readonly CustomAuthStateProvider _authProvider;

    public OrderApiService(
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

    // ── SESSIONS ─────────────────────────────────────────────────────

    public async Task<(bool Success, SessionModel? Session, string Error)> OpenSessionAsync(int tableId)
    {
        await AttachTokenAsync();
        var res = await _http.PostAsJsonAsync("/api/sessions", new { TableId = tableId });
        if (res.IsSuccessStatusCode)
        {
            var json = await res.Content.ReadAsStringAsync();
            Console.WriteLine($"Session response JSON: {json}"); // debug
            var session = System.Text.Json.JsonSerializer.Deserialize<SessionModel>(json,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return (true, session, "");
        }
        var error = await res.Content.ReadAsStringAsync();
        return (false, null, error);
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
            Console.WriteLine($"GetActiveSession failed: {response.StatusCode} - {error}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetActiveSession exception: {ex.Message}");
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

    // ── ORDERS ───────────────────────────────────────────────────────

    public async Task<(bool Success, OrderModel? Order, string Error)> PlaceOrderAsync(int sessionId, List<CartItem> items, string? note = null)
    {
        var request = new
        {
            SessionId = sessionId,
            Note = note,
            Items = items.Select(i => new
            {
                MenuItemId = i.MenuItemId,
                VarientId = i.VarientId,
                Quantity = i.Quantity,
                Note = i.Note
            }).ToList()
        };

        // No token needed — AllowAnonymous for now
        var res = await _http.PostAsJsonAsync("/api/orders", request);
        if (res.IsSuccessStatusCode)
        {
            var order = await res.Content.ReadFromJsonAsync<OrderModel>();
            return (true, order, "");
        }
        var error = await res.Content.ReadAsStringAsync();
        return (false, null, error);
    }

    public async Task<List<OrderModel>> GetSessionOrdersAsync(int sessionId)
    {
        await AttachTokenAsync();
        return await _http.GetFromJsonAsync<List<OrderModel>>($"/api/orders/session/{sessionId}")
               ?? new List<OrderModel>();
    }

    // ── KITCHEN ──────────────────────────────────────────────────────

    public async Task<List<OrderModel>> GetKitchenOrdersAsync()
    {
        await AttachTokenAsync();
        return await _http.GetFromJsonAsync<List<OrderModel>>("/api/orders/kitchen")
               ?? new List<OrderModel>();
    }

    public async Task<bool> UpdateOrderStatusAsync(int orderId, string status)
    {
        await AttachTokenAsync();
        var res = await _http.PatchAsJsonAsync($"/api/orders/{orderId}/status", new { Status = status });
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateOrderItemStatusAsync(int itemId, string status)
    {
        await AttachTokenAsync();
        var res = await _http.PatchAsJsonAsync($"/api/orders/items/{itemId}/status", new { Status = status });
        return res.IsSuccessStatusCode;
    }
}