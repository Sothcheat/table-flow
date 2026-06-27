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
                    .AsNoTracking()
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

            // GET all active orders — Kitchen (active = Open session)
            orders.MapGet("/kitchen", async (
                [FromServices] IDbContextFactory<AppDbContext> factory) =>
            {
                await using var db = await factory.CreateDbContextAsync();

                var orderList = await db.Orders
                    .AsNoTracking()
                    .Include(o => o.OrderItems)
                        .ThenInclude(i => i.MenuItem)
                    .Include(o => o.OrderItems)
                        .ThenInclude(i => i.Varient)
                    .Include(o => o.TableSession)
                        .ThenInclude(s => s.Table)
                    .Where(o => o.TableSession.SessionStatus == SessionStatus.Open
                             && (o.OrderStatus == OrderStatus.Pending || o.OrderStatus == OrderStatus.Preparing))
                    .OrderBy(o => o.CreatedAt)
                    .ToListAsync();

                var response = orderList.Select(MapToResponse).ToList();
                return Results.Ok(response);
            }).RequireAuthorization("KitchenOnly");

            // GET orders waiting for cashier pickup — Ready status, Open session
            orders.MapGet("/ready", async (
                [FromServices] IDbContextFactory<AppDbContext> factory) =>
            {
                await using var db = await factory.CreateDbContextAsync();

                var readyOrders = await db.Orders
                    .AsNoTracking()
                    .Include(o => o.OrderItems)
                        .ThenInclude(i => i.MenuItem)
                    .Include(o => o.OrderItems)
                        .ThenInclude(i => i.Varient)
                    .Include(o => o.TableSession)
                        .ThenInclude(s => s.Table)
                    .Where(o => o.TableSession.SessionStatus == SessionStatus.Open
                             && o.OrderStatus == OrderStatus.Ready)
                    .OrderBy(o => o.CreatedAt)
                    .ToListAsync();

                return Results.Ok(readyOrders.Select(MapToResponse).ToList());
            }).RequireAuthorization("CashierOnly");

            // GET paginated history — closed sessions or served orders, with server-side filtering
            orders.MapGet("/history", async (
                [FromServices] IDbContextFactory<AppDbContext> factory,
                [FromQuery] DateTimeOffset? from,
                [FromQuery] DateTimeOffset? to,
                [FromQuery] int? tableNumber,
                [FromQuery] string? search,
                [FromQuery] int page = 1,
                [FromQuery] int pageSize = 50) =>
            {
                page = Math.Max(1, page);
                pageSize = Math.Clamp(pageSize, 1, 100);

                await using var db = await factory.CreateDbContextAsync();

                var query = db.Orders
                    .AsNoTracking()
                    .Where(o => o.OrderStatus == OrderStatus.Ready || o.OrderStatus == OrderStatus.Served);

                if (from.HasValue)
                    query = query.Where(o => o.CreatedAt >= from.Value.UtcDateTime);
                if (to.HasValue)
                    query = query.Where(o => o.CreatedAt < to.Value.UtcDateTime);
                if (tableNumber.HasValue)
                    query = query.Where(o => o.TableSession.Table.TableNumber == tableNumber.Value);
                if (!string.IsNullOrWhiteSpace(search))
                    query = query.Where(o => o.OrderNumber.Contains(search));

                var totalCount = await query.CountAsync();
                var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize));

                var orders = await query
                    .OrderByDescending(o => o.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Include(o => o.OrderItems)
                        .ThenInclude(i => i.MenuItem)
                    .Include(o => o.OrderItems)
                        .ThenInclude(i => i.Varient)
                    .Include(o => o.TableSession)
                        .ThenInclude(s => s.Table)
                    .AsSplitQuery()
                    .ToListAsync();

                return Results.Ok(new OrderHistoryPageResponse(
                    orders.Select(MapToResponse).ToList(),
                    totalCount,
                    page,
                    pageSize,
                    totalPages
                ));
            }).RequireAuthorization("KitchenOnly");

            // GET orders in open sessions that have at least one Unavailable item — Cashier warning banner
            orders.MapGet("/unavailable-alerts", async (
                [FromServices] IDbContextFactory<AppDbContext> factory) =>
            {
                await using var db = await factory.CreateDbContextAsync();
                var result = await db.Orders
                    .AsNoTracking()
                    .Include(o => o.OrderItems).ThenInclude(i => i.MenuItem)
                    .Include(o => o.OrderItems).ThenInclude(i => i.Varient)
                    .Include(o => o.TableSession).ThenInclude(s => s.Table)
                    .Where(o => o.TableSession.SessionStatus == SessionStatus.Open
                             && o.OrderItems.Any(i => i.OrderItemStatus == OrderItemStatus.Unavailable))
                    .OrderBy(o => o.CreatedAt)
                    .AsSplitQuery()
                    .ToListAsync();
                return Results.Ok(result.Select(MapToResponse).ToList());
            }).RequireAuthorization("CashierOnly");

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

                // Batch-load all referenced menu items and variants — 2 queries instead of N*2
                var menuItemIds = request.Items.Select(i => i.MenuItemId).Distinct().ToList();
                var variantIds  = request.Items
                    .Where(i => i.VarientId.HasValue)
                    .Select(i => i.VarientId!.Value)
                    .Distinct()
                    .ToList();

                var menuItemMap = await db.MenuItems
                    .Where(m => menuItemIds.Contains(m.Id))
                    .ToDictionaryAsync(m => m.Id);

                var variantMap = variantIds.Count > 0
                    ? await db.MenuItemVarients
                        .Where(v => variantIds.Contains(v.Id))
                        .ToDictionaryAsync(v => v.Id)
                    : new Dictionary<int, MenuItemVarient>();

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

                // Build order items using dictionary lookups — zero extra DB calls
                foreach (var item in request.Items)
                {
                    if (!menuItemMap.TryGetValue(item.MenuItemId, out var menuItem))
                        return Results.BadRequest($"Menu item {item.MenuItemId} not found.");

                    decimal unitPrice = menuItem.BasePrice;
                    if (item.VarientId.HasValue)
                    {
                        if (!variantMap.TryGetValue(item.VarientId.Value, out var variant))
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

            // PATCH update order status — Kitchen advances Pending→Preparing→Ready
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

            // PATCH update order item status — Kitchen marks Waiting→Preparing→Done
            orders.MapPatch("/items/{id:int}/status", async (
                int id,
                [FromBody] UpdateOrderItemStatusRequest request,
                [FromServices] IDbContextFactory<AppDbContext> factory) =>
            {
                await using var db = await factory.CreateDbContextAsync();

                var item = await db.OrderItems
                    .Include(i => i.MenuItem)
                        .ThenInclude(m => m.MenuItemVarients)
                    .Include(i => i.Varient)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (item is null) return Results.NotFound();

                if (!Enum.TryParse<OrderItemStatus>(request.Status, ignoreCase: true, out var newStatus))
                    return Results.BadRequest("Invalid order item status.");

                item.OrderItemStatus = newStatus;

                // Surgical unavailability: only mark what was actually ordered
                if (newStatus == OrderItemStatus.Unavailable)
                {
                    if (item.Varient is not null)
                    {
                        // Mark only the specific ordered variant unavailable
                        item.Varient.IsAvailable = false;
                        // Recompute item-level flag from remaining variants
                        item.MenuItem.IsAvailable = item.MenuItem.MenuItemVarients.Any(v => v.IsAvailable);
                    }
                    else
                    {
                        // No variant — mark the whole item unavailable
                        item.MenuItem.IsAvailable = false;
                    }
                }

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
            }).RequireAuthorization("KitchenOnly");
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
            order.OrderItems
                .Where(i => i.OrderItemStatus != OrderItemStatus.Unavailable)
                .Sum(i => i.UnitPrice * i.Quantity)
        );
    }
}
