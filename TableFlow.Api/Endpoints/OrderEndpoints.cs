using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TableFlow.Api.Data;
using TableFlow.Api.Data.Entities;
using TableFlow.Api.DTOs;

namespace TableFlow.Api.Endpoints
{
    public static class OrderEndpoints
    {
        public static void MapOrderEndpoints(this WebApplication app)
        {
            var orders = app.MapGroup("/api/orders").RequireAuthorization();

            // GET all orders for a session
            orders.MapGet("/session/{sessionId:int}", async (
                int sessionId,
                [FromServices] IDbContextFactory<AppDbContext> factory) =>
            {
                await using var db = await factory.CreateDbContextAsync();

                var exists = await db.TableSessions.AnyAsync(s => s.Id == sessionId);
                if (!exists) return Results.NotFound("Session not found.");

                var orderList = await db.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(i => i.MenuItem)
                    .Include(o => o.OrderItems)
                        .ThenInclude(i => i.Varient)
                    .Include(o => o.TableSession)
                        .ThenInclude(s => s.Table)
                    .Where(o => o.SessionId == sessionId)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();

                var response = orderList.Select(MapToResponse).ToList();
                return Results.Ok(response);
            }).AllowAnonymous();

            // GET single order by id
            orders.MapGet("/{id:int}", async (
                int id,
                [FromServices] IDbContextFactory<AppDbContext> factory) =>
            {
                await using var db = await factory.CreateDbContextAsync();

                var order = await db.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(i => i.MenuItem)
                    .Include(o => o.OrderItems)
                        .ThenInclude(i => i.Varient)
                    .Include(o => o.TableSession)
                        .ThenInclude(s => s.Table)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order is null) return Results.NotFound();
                return Results.Ok(MapToResponse(order));
            });

            // GET all active orders — Kitchen (active = the order's session is still Open)
            orders.MapGet("/kitchen", async (
                [FromServices] IDbContextFactory<AppDbContext> factory) =>
            {
                await using var db = await factory.CreateDbContextAsync();

                var orderList = await db.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(i => i.MenuItem)
                    .Include(o => o.OrderItems)
                        .ThenInclude(i => i.Varient)
                    .Include(o => o.TableSession)
                        .ThenInclude(s => s.Table)
                    .Where(o => o.TableSession.SessionStatus == SessionStatus.Open)
                    .OrderBy(o => o.CreatedAt)
                    .ToListAsync();

                var response = orderList.Select(MapToResponse).ToList();
                return Results.Ok(response);
            }).RequireAuthorization("KitchenOnly");

            // POST create order — called when customer places order
            orders.MapPost("/", async (
                [FromBody] CreateOrderRequest request,
                [FromServices] IDbContextFactory<AppDbContext> factory) =>
            {
                await using var db = await factory.CreateDbContextAsync();

                // Validate session exists and is open
                var session = await db.TableSessions.FindAsync(request.SessionId);
                if (session is null) return Results.NotFound("Session not found.");
                if (session.SessionStatus == SessionStatus.Closed)
                    return Results.Conflict("Session is already closed.");

                // Validate items
                if (request.Items is null || request.Items.Count == 0)
                    return Results.BadRequest("Order must have at least one item.");

                // Generate order number
                var orderCount = await db.Orders
                    .Where(o => o.SessionId == request.SessionId)
                    .CountAsync();
                var orderNumber = $"ORD-{request.SessionId:D4}-{(orderCount + 1):D3}";

                var order = new Order
                {
                    SessionId = request.SessionId,
                    OrderNumber = orderNumber,
                    OrderStatus = OrderStatus.Pending,
                    Note = request.Note,
                    CreatedAt = DateTime.UtcNow
                };

                // Build order items
                foreach (var item in request.Items)
                {
                    var menuItem = await db.MenuItems.FindAsync(item.MenuItemId);
                    if (menuItem is null)
                        return Results.BadRequest($"Menu item {item.MenuItemId} not found.");

                    // Determine unit price
                    decimal unitPrice = menuItem.BasePrice;
                    if (item.VarientId.HasValue)
                    {
                        var variant = await db.MenuItemVarients.FindAsync(item.VarientId.Value);
                        if (variant is null)
                            return Results.BadRequest($"Variant {item.VarientId} not found.");
                        unitPrice = variant.Price;
                    }

                    order.OrderItems.Add(new OrderItem
                    {
                        MenuItemId = item.MenuItemId,
                        VarientId = item.VarientId,
                        Quantity = item.Quantity,
                        UnitPrice = unitPrice,
                        OrderItemStatus = OrderItemStatus.Waiting,
                        Note = item.Note
                    });
                }

                db.Orders.Add(order);
                await db.SaveChangesAsync();

                // Reload with includes for response
                var created = await db.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(i => i.MenuItem)
                    .Include(o => o.OrderItems)
                        .ThenInclude(i => i.Varient)
                    .Include(o => o.TableSession)
                        .ThenInclude(s => s.Table)
                    .FirstAsync(o => o.Id == order.Id);

                return Results.Created($"/api/orders/{order.Id}", MapToResponse(created));
            }).AllowAnonymous(); // TODO: Replace with session validation when QR flow is built

            // PATCH update order status — Kitchen
            orders.MapPatch("/{id:int}/status", async (
                int id,
                [FromBody] UpdateOrderStatusRequest request,
                [FromServices] IDbContextFactory<AppDbContext> factory) =>
            {
                await using var db = await factory.CreateDbContextAsync();

                var order = await db.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(i => i.MenuItem)
                    .Include(o => o.OrderItems)
                        .ThenInclude(i => i.Varient)
                    .Include(o => o.TableSession)
                        .ThenInclude(s => s.Table)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order is null) return Results.NotFound();

                if (!Enum.TryParse<OrderStatus>(request.Status, ignoreCase: true, out var newStatus))
                    return Results.BadRequest("Invalid order status.");

                order.OrderStatus = newStatus;
                await db.SaveChangesAsync();

                return Results.Ok(MapToResponse(order));
            }).RequireAuthorization("KitchenOnly");

            // PATCH update order item status — Kitchen
            orders.MapPatch("/items/{id:int}/status", async (
                int id,
                [FromBody] UpdateOrderItemStatusRequest request,
                [FromServices] IDbContextFactory<AppDbContext> factory) =>
            {
                await using var db = await factory.CreateDbContextAsync();

                var item = await db.OrderItems
                    .Include(i => i.MenuItem)
                    .Include(i => i.Varient)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (item is null) return Results.NotFound();

                if (!Enum.TryParse<OrderItemStatus>(request.Status, ignoreCase: true, out var newStatus))
                    return Results.BadRequest("Invalid order item status.");

                item.OrderItemStatus = newStatus;
                await db.SaveChangesAsync();

                return Results.Ok(new OrderItemResponse(
                    item.Id,
                    item.OrderId,
                    item.MenuItemId,
                    item.MenuItem.ItemName,
                    item.VarientId,
                    item.Varient?.VarientName,
                    item.Quantity,
                    item.UnitPrice,
                    item.UnitPrice * item.Quantity,
                    item.OrderItemStatus.ToString(),
                    item.Note
                ));
            }).RequireAuthorization("KitchenOrCashier");
        }

        private static OrderResponse MapToResponse(Order order) => new(
            order.Id,
            order.SessionId,
            order.TableSession.Table.TableNumber, // i add this to make it easier for the kitchen to see which table the order is for without having to look up the session
            order.OrderNumber,
            order.OrderStatus.ToString(),
            order.CreatedAt,
            order.Note,
            order.OrderItems.Select(i => new OrderItemResponse(
                i.Id,
                i.OrderId,
                i.MenuItemId,
                i.MenuItem.ItemName,
                i.VarientId,
                i.Varient?.VarientName,
                i.Quantity,
                i.UnitPrice,
                i.UnitPrice * i.Quantity,
                i.OrderItemStatus.ToString(),
                i.Note
            )).ToList(),
            order.OrderItems.Sum(i => i.UnitPrice * i.Quantity)
        );
    }
}
