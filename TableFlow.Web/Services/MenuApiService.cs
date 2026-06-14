using Microsoft.AspNetCore.Components.Authorization;
using TableFlow.Web.Auth;
using TableFlow.Web.Models;

namespace TableFlow.Web.Services;

public class MenuApiService
{
    private readonly HttpClient _http;
    private readonly CustomAuthStateProvider _authProvider;

    public MenuApiService(
        IHttpClientFactory httpClientFactory,
        AuthenticationStateProvider authProvider)
    {
        _http = httpClientFactory.CreateClient("TableFlowApi");
        _authProvider = (CustomAuthStateProvider)authProvider;
    }

    // ── HELPERS ──────────────────────────────────────────────────────

    private async Task AttachTokenAsync()
    {
        var token = await _authProvider.GetTokenAsync();
        _http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    // ── CATEGORIES ───────────────────────────────────────────────────

    public async Task<List<CategoryModel>> GetCategoriesAsync()
    {
        //await AttachTokenAsync();
        return await _http.GetFromJsonAsync<List<CategoryModel>>("/api/menu/categories")
               ?? new List<CategoryModel>();
    }

    public async Task<CategoryModel?> CreateCategoryAsync(CategoryFormModel model)
    {
        await AttachTokenAsync();
        var res = await _http.PostAsJsonAsync("/api/menu/categories", model);
        if (!res.IsSuccessStatusCode) return null;
        return await res.Content.ReadFromJsonAsync<CategoryModel>();
    }

    public async Task<CategoryModel?> UpdateCategoryAsync(int id, CategoryFormModel model)
    {
        await AttachTokenAsync();
        var res = await _http.PutAsJsonAsync($"/api/menu/categories/{id}", model);
        if (!res.IsSuccessStatusCode) return null;
        return await res.Content.ReadFromJsonAsync<CategoryModel>();
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        await AttachTokenAsync();
        var res = await _http.DeleteAsync($"/api/menu/categories/{id}");
        return res.IsSuccessStatusCode;
    }

    // ── MENU ITEMS ───────────────────────────────────────────────────

    public async Task<List<MenuItemModel>> GetMenuItemsAsync()
    {
        //await AttachTokenAsync();
        return await _http.GetFromJsonAsync<List<MenuItemModel>>("/api/menu/items")
               ?? new List<MenuItemModel>();
    }

    public async Task<List<MenuItemModel>> GetMenuItemsByCategoryAsync(int categoryId)
    {
        await AttachTokenAsync();
        return await _http.GetFromJsonAsync<List<MenuItemModel>>($"/api/menu/items/by-category/{categoryId}")
               ?? new List<MenuItemModel>();
    }

    public async Task<MenuItemModel?> CreateMenuItemAsync(MenuItemFormModel model)
    {
        await AttachTokenAsync();
        var res = await _http.PostAsJsonAsync("/api/menu/items", model);
        if (!res.IsSuccessStatusCode) return null;
        return await res.Content.ReadFromJsonAsync<MenuItemModel>();
    }

    public async Task<MenuItemModel?> UpdateMenuItemAsync(int id, MenuItemFormModel model)
    {
        await AttachTokenAsync();
        var res = await _http.PutAsJsonAsync($"/api/menu/items/{id}", model);
        if (!res.IsSuccessStatusCode) return null;
        return await res.Content.ReadFromJsonAsync<MenuItemModel>();
    }

    public async Task<bool> DeleteMenuItemAsync(int id)
    {
        await AttachTokenAsync();
        var res = await _http.DeleteAsync($"/api/menu/items/{id}");
        return res.IsSuccessStatusCode;
    }

    public async Task<MenuItemModel?> GetMenuItemByIdAsync(int id)
    {
        return await _http.GetFromJsonAsync<MenuItemModel>($"/api/menu/items/{id}");
    }

    public async Task<bool> UpdateItemAvailabilityAsync(int id, bool isAvailable)
    {
        await AttachTokenAsync();
        var res = await _http.PatchAsJsonAsync(
            $"/api/menu/items/{id}/availability",
            new { IsAvailable = isAvailable });
        return res.IsSuccessStatusCode;
    }

    // ── VARIANTS ─────────────────────────────────────────────────────

    public async Task<List<VarientModel>> GetVarientsAsync(int menuItemId)
    {
        //await AttachTokenAsync();
        return await _http.GetFromJsonAsync<List<VarientModel>>($"/api/menu/items/{menuItemId}/varients")
               ?? new List<VarientModel>();
    }

    public async Task<VarientModel?> CreateVarientAsync(int menuItemId, VarientFormModel model)
    {
        await AttachTokenAsync();
        var res = await _http.PostAsJsonAsync($"/api/menu/items/{menuItemId}/varients", model);
        if (!res.IsSuccessStatusCode) return null;
        return await res.Content.ReadFromJsonAsync<VarientModel>();
    }

    public async Task<VarientModel?> UpdateVarientAsync(int menuItemId, int varientId, VarientFormModel model)
    {
        await AttachTokenAsync();
        var res = await _http.PutAsJsonAsync($"/api/menu/items/{menuItemId}/varients/{varientId}", model);
        if (!res.IsSuccessStatusCode) return null;
        return await res.Content.ReadFromJsonAsync<VarientModel>();
    }

    public async Task<bool> DeleteVarientAsync(int menuItemId, int varientId)
    {
        await AttachTokenAsync();
        var res = await _http.DeleteAsync($"/api/menu/items/{menuItemId}/varients/{varientId}");
        return res.IsSuccessStatusCode;
    }
}
