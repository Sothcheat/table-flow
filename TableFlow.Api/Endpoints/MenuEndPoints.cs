using TableFlow.Api.Data;          
using TableFlow.Api.Data.Entities;  
using TableFlow.Api.DTOs;           
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace TableFlow.Api.Endpoints;

public static class MenuEndpoints
{
    public static void MapMenuEndpoints(this WebApplication app)
    {
        var menu = app.MapGroup("/api/menu").RequireAuthorization("AdminOnly");

        // ── CATEGORIES ──────────────────────────────────────────────

        menu.MapGet("/categories", async ([FromServices] IDbContextFactory<AppDbContext> factory) =>
        {
            await using var db = await factory.CreateDbContextAsync();

            var categories = await db.Categories
                .Include(c => c.MenuItems)
                .OrderBy(c => c.DisplayOrder)
                .Select(c => new CategoryResponse(
                    c.Id,
                    c.CategoryName,
                    c.DisplayOrder,
                    c.MenuItems.Count))
                .ToListAsync();
            return Results.Ok(categories);
        }).AllowAnonymous();

        menu.MapGet("/categories/{id:int}", async (int id, [FromServices] IDbContextFactory<AppDbContext> factory) =>
        {
            await using var db = await factory.CreateDbContextAsync();
            var c = await db.Categories
                .Include(c => c.MenuItems)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (c is null) return Results.NotFound();
            return Results.Ok(new CategoryResponse(c.Id, c.CategoryName, c.DisplayOrder, c.MenuItems.Count));
        });

        menu.MapPost("/categories", async (CreateCategoryRequest req, [FromServices] IDbContextFactory<AppDbContext> factory) =>
        {
            await using var db = await factory.CreateDbContextAsync();

            var toShift = await db.Categories
                .Where(c => c.DisplayOrder >= req.DisplayOrder)
                .ToListAsync();
            foreach (var c in toShift)
                c.DisplayOrder++;

            var category = new Category
            {
                CategoryName = req.CategoryName,
                DisplayOrder = req.DisplayOrder
            };

            db.Categories.Add(category);
            await db.SaveChangesAsync();

            return Results.Created($"/api/menu/categories/{category.Id}",
                new CategoryResponse(category.Id, category.CategoryName, category.DisplayOrder, 0));
        });

        menu.MapPut("/categories/{id:int}", async (int id, UpdateCategoryRequest req, [FromServices] IDbContextFactory<AppDbContext> factory) =>
        {
            await using var db = await factory.CreateDbContextAsync();
            var category = await db.Categories.FindAsync(id);
            if (category is null) return Results.NotFound();

            if (category.DisplayOrder != req.DisplayOrder)
            {
                var toShift = await db.Categories
                    .Where(c => c.DisplayOrder >= req.DisplayOrder && c.Id != id)
                    .ToListAsync();
                foreach (var c in toShift)
                    c.DisplayOrder++;
            }

            category.CategoryName = req.CategoryName;
            category.DisplayOrder = req.DisplayOrder;
            await db.SaveChangesAsync();

            return Results.Ok(new CategoryResponse(category.Id, category.CategoryName, category.DisplayOrder, 0));
        });

        menu.MapDelete("/categories/{id:int}", async (int id, [FromServices] IDbContextFactory<AppDbContext> factory) =>
        {
            await using var db = await factory.CreateDbContextAsync();
            var category = await db.Categories
                .Include(c => c.MenuItems)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (category is null) return Results.NotFound();

            if (category.MenuItems.Count > 0)
                return Results.Conflict("Cannot delete a category that   menu items. Remove or reassign items first.");

            db.Categories.Remove(category);

            var remaining = await db.Categories
                .Where(c => c.Id != id)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            for (int i = 0; i < remaining.Count; i++)
                remaining[i].DisplayOrder = i + 1;

            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        // ── MENU ITEMS ───────────────────────────────────────────────

        menu.MapGet("/items", async ([FromServices] IDbContextFactory<AppDbContext> factory) =>
        {
            await using var db = await factory.CreateDbContextAsync();

            // Step 1 — fetch from DB first (AsNoTracking for read-only perf)
            var items = await db.MenuItems
                .AsNoTracking()
                .Include(i => i.Category)
                .Include(i => i.MenuItemVarients)
                .OrderBy(i => i.Category.DisplayOrder)
                .ThenBy(i => i.ItemName)
                .ToListAsync();  // ← materialize to C# list first

            // Step 2 — map to DTOs in memory (no EF translation issue)
            var response = items.Select(i => new MenuItemResponse(
                i.Id, i.ItemName, i.BasePrice,
                i.IsAvailable, i.HasVarient, i.ImageUrl,
                i.CategoryId, i.Category.CategoryName,
                i.MenuItemVarients
                    .Select(v => new MenuItemVarientResponse(
                        v.Id, v.MenuItemId, v.VarientName, v.Price, v.IsAvailable))
                    .ToList()
            )).ToList();

            return Results.Ok(response);
        }).AllowAnonymous();

        menu.MapGet("/items/by-category/{categoryId:int}", async (int categoryId, [FromServices] IDbContextFactory<AppDbContext> factory) =>
        {
            await using var db = await factory.CreateDbContextAsync();

            var items = await db.MenuItems
                .AsNoTracking()
                .Include(i => i.Category)
                .Include(i => i.MenuItemVarients)
                .Where(i => i.CategoryId == categoryId)
                .OrderBy(i => i.ItemName)
                .ToListAsync();

            var response = items.Select(i => new MenuItemResponse(
                    i.Id, i.ItemName, i.BasePrice,
                    i.IsAvailable, i.HasVarient, i.ImageUrl,
                    i.CategoryId, i.Category.CategoryName,
                    i.MenuItemVarients.Select(v => new MenuItemVarientResponse(v.Id, v.MenuItemId, v.VarientName, v.Price, v.IsAvailable)).ToList()
                    )).ToList();

            return Results.Ok(response);
        }).AllowAnonymous();

        menu.MapGet("/items/{id:int}", async (int id, [FromServices] IDbContextFactory<AppDbContext> factory) =>
        {
            await using var db = await factory.CreateDbContextAsync();
            var i = await db.MenuItems
                .Include(i => i.Category)
                .Include(i => i.MenuItemVarients)
                .FirstOrDefaultAsync(i => i.Id == id);
            if (i is null) return Results.NotFound();

            return Results.Ok(new MenuItemResponse(
                i.Id, i.ItemName, i.BasePrice,
                i.IsAvailable, i.HasVarient, i.ImageUrl,
                i.CategoryId, i.Category.CategoryName,
                i.MenuItemVarients.Select(v => new MenuItemVarientResponse(v.Id, v.MenuItemId, v.VarientName, v.Price, v.IsAvailable)).ToList()
                ));
        }).AllowAnonymous();

        menu.MapPost("/items", async (CreateMenuItemRequest req, [FromServices] IDbContextFactory<AppDbContext> factory) =>
        {
            await using var db = await factory.CreateDbContextAsync();

            // Validate category exists
            var categoryExists = await db.Categories.AnyAsync(c => c.Id == req.CategoryId);
            if (!categoryExists) return Results.BadRequest("Category not found.");

            var item = new MenuItem
            {
                ItemName = req.ItemName,
                BasePrice = req.BasePrice,
                IsAvailable = req.IsAvailable,  
                HasVarient = req.HasVarient,
                ImageUrl = req.ImageUrl,
                CategoryId = req.CategoryId
            };
            db.MenuItems.Add(item);
            await db.SaveChangesAsync();

            await db.Entry(item).Reference(i => i.Category).LoadAsync();
            return Results.Created($"/api/menu/items/{item.Id}",
                new MenuItemResponse(item.Id, item.ItemName, item.BasePrice,
                    item.IsAvailable, item.HasVarient, item.ImageUrl, item.CategoryId, item.Category.CategoryName,
                    item.MenuItemVarients.Select(v => new MenuItemVarientResponse(v.Id, v.MenuItemId, v.VarientName, v.Price, v.IsAvailable)).ToList()
                    ));
        });

        menu.MapPut("/items/{id:int}", async (int id, UpdateMenuItemRequest req, [FromServices] IDbContextFactory<AppDbContext> factory) =>
        {
            await using var db = await factory.CreateDbContextAsync();
            var item = await db.MenuItems.Include(i => i.Category).FirstOrDefaultAsync(i => i.Id == id);
            if (item is null) return Results.NotFound();

            var categoryExists = await db.Categories.AnyAsync(c => c.Id == req.CategoryId);
            if (!categoryExists) return Results.BadRequest("Category not found.");

            item.ItemName = req.ItemName;
            item.BasePrice = req.BasePrice;
            item.IsAvailable = req.IsAvailable;
            item.HasVarient = req.HasVarient;
            item.ImageUrl = req.ImageUrl;
            item.CategoryId = req.CategoryId;

            await db.SaveChangesAsync();
            await db.Entry(item).Reference(i => i.Category).LoadAsync();

            return Results.Ok(new MenuItemResponse(item.Id, item.ItemName, item.BasePrice,
                item.IsAvailable, item.HasVarient, item.ImageUrl, item.CategoryId, item.Category.CategoryName,
                item.MenuItemVarients.Select(v => new MenuItemVarientResponse(v.Id, v.MenuItemId, v.VarientName, v.Price, v.IsAvailable)).ToList()
                ));
        });

        menu.MapDelete("/items/{id:int}", async (int id, [FromServices] IDbContextFactory<AppDbContext> factory) =>
        {
            await using var db = await factory.CreateDbContextAsync();
            var item = await db.MenuItems.FindAsync(id);
            if (item is null) return Results.NotFound();
            db.MenuItems.Remove(item);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        // PATCH update item availability — Kitchen or Admin
        menu.MapPatch("/items/{id:int}/availability", async (
            int id,
            [FromBody] UpdateItemAvailabilityRequest req,
            [FromServices] IDbContextFactory<AppDbContext> factory) =>
        {
            await using var db = await factory.CreateDbContextAsync();
            var item = await db.MenuItems.FindAsync(id);
            if (item is null) return Results.NotFound();
            item.IsAvailable = req.IsAvailable;
            await db.SaveChangesAsync();
            return Results.Ok();
        }).RequireAuthorization("AdminOrKitchen");

        // ── VARIANTS ─────────────────────────────────────────────────

        menu.MapGet("/items/{itemId:int}/varients", async (int itemId, [FromServices] IDbContextFactory<AppDbContext> factory) =>
        {
            await using var db = await factory.CreateDbContextAsync();
            var exists = await db.MenuItems.AnyAsync(i => i.Id == itemId);
            if (!exists) return Results.NotFound();

            var varients = await db.MenuItemVarients
                .Where(v => v.MenuItemId == itemId)
                .OrderBy(v => v.Price)
                .Select(v => new MenuItemVarientResponse(
                    v.Id, v.MenuItemId, v.VarientName, v.Price, v.IsAvailable))
                .ToListAsync();
            return Results.Ok(varients);
        }).AllowAnonymous();

        menu.MapPost("/items/{itemId:int}/varients", async (int itemId, CreateMenuItemVarientRequest req, [FromServices] IDbContextFactory<AppDbContext> factory) =>
        {
            await using var db = await factory.CreateDbContextAsync();
            var menuItem = await db.MenuItems
                .Include(m => m.MenuItemVarients)
                .FirstOrDefaultAsync(m => m.Id == itemId);
            if (menuItem is null) return Results.NotFound();

            var varient = new MenuItemVarient
            {
                MenuItemId = itemId,
                VarientName = req.VarientName,
                Price = req.Price,
                IsAvailable = req.IsAvailable
            };
            db.MenuItemVarients.Add(varient);

            // Adding an available variant makes the item available
            if (req.IsAvailable)
                menuItem.IsAvailable = true;

            await db.SaveChangesAsync();
            return Results.Created($"/api/menu/items/{itemId}/varients/{varient.Id}",
                new MenuItemVarientResponse(varient.Id, varient.MenuItemId, varient.VarientName, varient.Price, varient.IsAvailable));
        });

        menu.MapPut("/items/{itemId:int}/varients/{varientId:int}", async (int itemId, int varientId, UpdateMenuItemVarientRequest req, [FromServices] IDbContextFactory<AppDbContext> factory) =>
        {
            await using var db = await factory.CreateDbContextAsync();
            var varient = await db.MenuItemVarients
                .Include(v => v.MenuItem)
                    .ThenInclude(m => m.MenuItemVarients)
                .FirstOrDefaultAsync(v => v.Id == varientId && v.MenuItemId == itemId);
            if (varient is null) return Results.NotFound();

            varient.VarientName = req.VarientName;
            varient.Price = req.Price;
            varient.IsAvailable = req.IsAvailable;

            // Item is available if at least one variant is available
            varient.MenuItem.IsAvailable = varient.MenuItem.MenuItemVarients.Any(v => v.IsAvailable);

            await db.SaveChangesAsync();
            return Results.Ok(new MenuItemVarientResponse(
                varient.Id, varient.MenuItemId, varient.VarientName, varient.Price, varient.IsAvailable));
        });

        menu.MapDelete("/items/{itemId:int}/varients/{varientId:int}", async (int itemId, int varientId, [FromServices] IDbContextFactory<AppDbContext> factory) =>
        {
            await using var db = await factory.CreateDbContextAsync();
            var varient = await db.MenuItemVarients
                .Include(v => v.MenuItem)
                    .ThenInclude(m => m.MenuItemVarients)
                .FirstOrDefaultAsync(v => v.Id == varientId && v.MenuItemId == itemId);
            if (varient is null) return Results.NotFound();
            db.MenuItemVarients.Remove(varient);

            // Recompute item availability from remaining variants
            var remaining = varient.MenuItem.MenuItemVarients.Where(v => v.Id != varientId).ToList();
            varient.MenuItem.IsAvailable = remaining.Count == 0 || remaining.Any(v => v.IsAvailable);

            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }
}
