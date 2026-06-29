using Microsoft.AspNetCore.Components.Authorization;
using System.Net;
using TableFlow.Web.Auth;
using TableFlow.Web.Models;

namespace TableFlow.Web.Services;

public class MenuApiService
{
    private readonly HttpClient _http;
    private readonly CustomAuthStateProvider _authProvider;
    private readonly UnauthorizedNotifier _notifier;

    public MenuApiService(
        IHttpClientFactory httpClientFactory,
        AuthenticationStateProvider authProvider,
        UnauthorizedNotifier notifier)
    {
        _http = httpClientFactory.CreateClient("TableFlowApi");
        _authProvider = (CustomAuthStateProvider)authProvider;
        _notifier = notifier;
    }

    // ── HELPERS ──────────────────────────────────────────────────────

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

    private async Task<T?> GetJsonAsync<T>(string url)
    {
        var res = await _http.GetAsync(url);
        if (await CheckUnauthorizedAsync(res)) return default;
        if (!res.IsSuccessStatusCode) return default;
        return await res.Content.ReadFromJsonAsync<T>(
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    // ── CATEGORIES ───────────────────────────────────────────────────

    public async Task<List<CategoryModel>> GetCategoriesAsync()
    {
        return await GetJsonAsync<List<CategoryModel>>("/api/menu/categories")
               ?? new List<CategoryModel>();
    }

    public async Task<CategoryModel?> CreateCategoryAsync(CategoryFormModel model)
    {
        await AttachTokenAsync();
        var res = await _http.PostAsJsonAsync("/api/menu/categories", model);
        if (await CheckUnauthorizedAsync(res)) return null;
        if (!res.IsSuccessStatusCode) return null;
        return await res.Content.ReadFromJsonAsync<CategoryModel>();
    }

    public async Task<CategoryModel?> UpdateCategoryAsync(int id, CategoryFormModel model)
    {
        await AttachTokenAsync();
        var res = await _http.PutAsJsonAsync($"/api/menu/categories/{id}", model);
        if (await CheckUnauthorizedAsync(res)) return null;
        if (!res.IsSuccessStatusCode) return null;
        return await res.Content.ReadFromJsonAsync<CategoryModel>();
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        await AttachTokenAsync();
        var res = await _http.DeleteAsync($"/api/menu/categories/{id}");
        if (await CheckUnauthorizedAsync(res)) return false;
        return res.IsSuccessStatusCode;
    }

    // ── MENU ITEMS ───────────────────────────────────────────────────

    public async Task<List<MenuItemModel>> GetMenuItemsAsync()
    {
        return await GetJsonAsync<List<MenuItemModel>>("/api/menu/items")
               ?? new List<MenuItemModel>();
    }

    public async Task<List<MenuItemModel>> GetMenuItemsByCategoryAsync(int categoryId)
    {
        await AttachTokenAsync();
        return await GetJsonAsync<List<MenuItemModel>>($"/api/menu/items/by-category/{categoryId}")
               ?? new List<MenuItemModel>();
    }

    public async Task<MenuItemModel?> CreateMenuItemAsync(MenuItemFormModel model)
    {
        await AttachTokenAsync();
        var res = await _http.PostAsJsonAsync("/api/menu/items", model);
        if (await CheckUnauthorizedAsync(res)) return null;
        if (!res.IsSuccessStatusCode) return null;
        return await res.Content.ReadFromJsonAsync<MenuItemModel>();
    }

    public async Task<MenuItemModel?> UpdateMenuItemAsync(int id, MenuItemFormModel model)
    {
        await AttachTokenAsync();
        var res = await _http.PutAsJsonAsync($"/api/menu/items/{id}", model);
        if (await CheckUnauthorizedAsync(res)) return null;
        if (!res.IsSuccessStatusCode) return null;
        return await res.Content.ReadFromJsonAsync<MenuItemModel>();
    }

    public async Task<(bool Success, string Error)> DeleteMenuItemAsync(int id)
    {
        await AttachTokenAsync();
        var res = await _http.DeleteAsync($"/api/menu/items/{id}");
        if (await CheckUnauthorizedAsync(res)) return (false, "Unauthorized");
        if (res.IsSuccessStatusCode) return (true, "");
        var error = await res.Content.ReadAsStringAsync();
        return (false, error);
    }

    public async Task<MenuItemModel?> GetMenuItemByIdAsync(int id)
    {
        return await GetJsonAsync<MenuItemModel>($"/api/menu/items/{id}");
    }

    public async Task<bool> UpdateItemAvailabilityAsync(int id, bool isAvailable)
    {
        await AttachTokenAsync();
        var res = await _http.PatchAsJsonAsync(
            $"/api/menu/items/{id}/availability",
            new { IsAvailable = isAvailable });
        if (await CheckUnauthorizedAsync(res)) return false;
        return res.IsSuccessStatusCode;
    }

    // ── VARIANTS ─────────────────────────────────────────────────────

    public async Task<List<VarientModel>> GetVarientsAsync(int menuItemId)
    {
        return await GetJsonAsync<List<VarientModel>>($"/api/menu/items/{menuItemId}/varients")
               ?? new List<VarientModel>();
    }

    public async Task<VarientModel?> CreateVarientAsync(int menuItemId, VarientFormModel model)
    {
        await AttachTokenAsync();
        var res = await _http.PostAsJsonAsync($"/api/menu/items/{menuItemId}/varients", model);
        if (await CheckUnauthorizedAsync(res)) return null;
        if (!res.IsSuccessStatusCode) return null;
        return await res.Content.ReadFromJsonAsync<VarientModel>();
    }

    public async Task<VarientModel?> UpdateVarientAsync(int menuItemId, int varientId, VarientFormModel model)
    {
        await AttachTokenAsync();
        var res = await _http.PutAsJsonAsync($"/api/menu/items/{menuItemId}/varients/{varientId}", model);
        if (await CheckUnauthorizedAsync(res)) return null;
        if (!res.IsSuccessStatusCode) return null;
        return await res.Content.ReadFromJsonAsync<VarientModel>();
    }

    public async Task<bool> DeleteVarientAsync(int menuItemId, int varientId)
    {
        await AttachTokenAsync();
        var res = await _http.DeleteAsync($"/api/menu/items/{menuItemId}/varients/{varientId}");
        if (await CheckUnauthorizedAsync(res)) return false;
        return res.IsSuccessStatusCode;
    }
}
