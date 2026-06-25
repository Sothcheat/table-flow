using System.Net;
using Microsoft.AspNetCore.Components.Authorization;
using TableFlow.Web.Auth;
using TableFlow.Web.Models;

namespace TableFlow.Web.Services;

public class OrderApiService
{
    private readonly HttpClient _http;
    private readonly CustomAuthStateProvider _authProvider;
    private readonly UnauthorizedNotifier _notifier;

    public OrderApiService(
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

    // 403 = token is valid but doesn't have the right role / has gone stale in a way
    // the server rejects without it being a clean 401. Treated as "please log in again" too.
    private async Task<bool> CheckForbiddenAsync(HttpResponseMessage response)
    {
        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            await _notifier.NotifyAsync();
            return true;
        }
        return false;
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
        var res = await _http.GetAsync($"/api/orders/session/{sessionId}");
        if (await CheckUnauthorizedAsync(res)) return new List<OrderModel>();
        if (!res.IsSuccessStatusCode) return new List<OrderModel>();
        return await res.Content.ReadFromJsonAsync<List<OrderModel>>() ?? new List<OrderModel>();
    }

    public async Task<List<OrderModel>> GetSessionOrdersClientAsync(int sessionId)
    {
        var res = await _http.GetAsync($"/api/orders/session/{sessionId}");
        if (!res.IsSuccessStatusCode) return new List<OrderModel>();
        return await res.Content.ReadFromJsonAsync<List<OrderModel>>() ?? new List<OrderModel>();
    }

    // ── CASHIER ──────────────────────────────────────────────────────

    public async Task<List<OrderModel>> GetReadyOrdersAsync()
    {
        await AttachTokenAsync();
        var res = await _http.GetAsync("/api/orders/ready");
        if (await CheckUnauthorizedAsync(res)) return new List<OrderModel>();
        if (!res.IsSuccessStatusCode) return new List<OrderModel>();
        return await res.Content.ReadFromJsonAsync<List<OrderModel>>() ?? new List<OrderModel>();
    }

    public async Task<List<OrderModel>> GetUnavailableAlertOrdersAsync()
    {
        await AttachTokenAsync();
        var res = await _http.GetAsync("/api/orders/unavailable-alerts");
        if (await CheckUnauthorizedAsync(res)) return new List<OrderModel>();
        if (!res.IsSuccessStatusCode) return new List<OrderModel>();
        return await res.Content.ReadFromJsonAsync<List<OrderModel>>() ?? new List<OrderModel>();
    }

    // ── KITCHEN ──────────────────────────────────────────────────────

    public async Task<List<OrderModel>> GetKitchenOrdersAsync()
    {
        await AttachTokenAsync();
        var res = await _http.GetAsync("/api/orders/kitchen");
        if (await CheckUnauthorizedAsync(res)) return new List<OrderModel>();
        if (!res.IsSuccessStatusCode) return new List<OrderModel>();
        return await res.Content.ReadFromJsonAsync<List<OrderModel>>() ?? new List<OrderModel>();
    }

    public async Task<(bool Success, List<OrderModel> Orders)> GetKitchenOrdersCheckedAsync()
    {
        await AttachTokenAsync();
        try
        {
            var res = await _http.GetAsync("/api/orders/kitchen");
            if (await CheckUnauthorizedAsync(res)) return (false, new List<OrderModel>());
            if (await CheckForbiddenAsync(res)) return (false, new List<OrderModel>());
            if (!res.IsSuccessStatusCode) return (false, new List<OrderModel>());
            var orders = await res.Content.ReadFromJsonAsync<List<OrderModel>>() ?? new List<OrderModel>();
            return (true, orders);
        }
        catch
        {
            return (false, new List<OrderModel>());
        }
    }

    public async Task<bool> UpdateOrderStatusAsync(int orderId, string status)
    {
        await AttachTokenAsync();
        var res = await _http.PatchAsJsonAsync($"/api/orders/{orderId}/status", new { Status = status });
        if (await CheckUnauthorizedAsync(res)) return false;
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateOrderItemStatusAsync(int itemId, string status)
    {
        await AttachTokenAsync();
        var res = await _http.PatchAsJsonAsync($"/api/orders/items/{itemId}/status", new { Status = status });
        if (await CheckUnauthorizedAsync(res)) return false;
        return res.IsSuccessStatusCode;
    }


    public async Task<(bool Success, OrderHistoryPageModel? Result, string? ErrorDetail)> GetKitchenHistoryPagedAsync(
        DateTimeOffset? from, DateTimeOffset? to, int? tableNumber, string? search, int page, int pageSize = 50)
    {
        await AttachTokenAsync();
        try
        {
            var qs = $"page={page}&pageSize={pageSize}";
            if (from.HasValue) qs += $"&from={Uri.EscapeDataString(from.Value.ToString("O"))}";
            if (to.HasValue)   qs += $"&to={Uri.EscapeDataString(to.Value.ToString("O"))}";
            if (tableNumber.HasValue) qs += $"&tableNumber={tableNumber}";
            if (!string.IsNullOrWhiteSpace(search)) qs += $"&search={Uri.EscapeDataString(search)}";

            var res = await _http.GetAsync($"/api/orders/history?{qs}");
            if (await CheckUnauthorizedAsync(res)) return (false, null, "401 Unauthorized");
            if (await CheckForbiddenAsync(res))    return (false, null, "403 Forbidden");
            if (!res.IsSuccessStatusCode)          return (false, null, $"{(int)res.StatusCode} {res.StatusCode}");

            var result = await res.Content.ReadFromJsonAsync<OrderHistoryPageModel>();
            return (true, result, null);
        }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }
}
