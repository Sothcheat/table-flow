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

    // ── KITCHEN ──────────────────────────────────────────────────────

    public async Task<List<OrderModel>> GetKitchenOrdersAsync()
    {
        await AttachTokenAsync();
        var res = await _http.GetAsync("/api/orders/kitchen");
        if (await CheckUnauthorizedAsync(res)) return new List<OrderModel>();
        if (!res.IsSuccessStatusCode) return new List<OrderModel>();
        return await res.Content.ReadFromJsonAsync<List<OrderModel>>() ?? new List<OrderModel>();
    }

    //// Same as above, but tells the caller WHY it came back empty — used by KitchenPage
    //// so a 401/403/network failure shows an error instead of silently looking like
    //// "no active orders". Added as a separate method so existing callers of
    //// GetKitchenOrdersAsync() are unaffected.
    //public async Task<(bool Success, List<OrderModel> Orders)> GetKitchenOrdersCheckedAsync()
    //{
    //    await AttachTokenAsync();
    //    try
    //    {
    //        var res = await _http.GetAsync("/api/orders/kitchen");
    //        if (await CheckUnauthorizedAsync(res)) return (false, new List<OrderModel>());
    //        if (await CheckForbiddenAsync(res)) return (false, new List<OrderModel>());
    //        if (!res.IsSuccessStatusCode) return (false, new List<OrderModel>());
    //        var orders = await res.Content.ReadFromJsonAsync<List<OrderModel>>() ?? new List<OrderModel>();
    //        return (true, orders);
    //    }
    //    catch
    //    {
    //        // network failure, API down, etc.
    //        return (false, new List<OrderModel>());
    //    }
    //}

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


    public async Task<List<OrderModel>> GetKitchenHistoryOrdersAsync()
    {
        await AttachTokenAsync();
        var res = await _http.GetAsync("/api/orders/history");
        if (await CheckUnauthorizedAsync(res)) return new List<OrderModel>();
        if (!res.IsSuccessStatusCode) return new List<OrderModel>();
        return await res.Content.ReadFromJsonAsync<List<OrderModel>>() ?? new List<OrderModel>();
    }

    // Same idea as GetKitchenOrdersCheckedAsync — tells the caller WHETHER the call
    // actually succeeded and what status code came back, so KitchenHistoryPage can
    // show a real error instead of an empty list looking like "no history exists".
    public async Task<(bool Success, List<OrderModel> Orders, string? ErrorDetail)> GetKitchenHistoryOrdersCheckedAsync()
    {
        await AttachTokenAsync();
        try
        {
            var res = await _http.GetAsync("/api/orders/history");
            if (await CheckUnauthorizedAsync(res)) return (false, new List<OrderModel>(), "401 Unauthorized");
            if (await CheckForbiddenAsync(res)) return (false, new List<OrderModel>(), "403 Forbidden");
            if (!res.IsSuccessStatusCode) return (false, new List<OrderModel>(), $"{(int)res.StatusCode} {res.StatusCode}");
            var orders = await res.Content.ReadFromJsonAsync<List<OrderModel>>() ?? new List<OrderModel>();
            return (true, orders, null);
        }
        catch (Exception ex)
        {
            return (false, new List<OrderModel>(), ex.Message);
        }
    }
}
