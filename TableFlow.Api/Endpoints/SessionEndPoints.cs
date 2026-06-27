using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TableFlow.Api.Data;
using TableFlow.Api.Data.Entities;
using TableFlow.Api.DTOs;

namespace TableFlow.Api.Endpoints
{
    public static class SessionEndpoints
    {
        public static void MapSessionEndpoints(this WebApplication app)
        {
            var sessions = app.MapGroup("/api/sessions").RequireAuthorization();

            // GET all sessions — paginated history list (Admin only)
            sessions.MapGet("", async (
                [FromServices] IDbContextFactory<AppDbContext> factory,
                int page = 1,
                int pageSize = 25,
                string filter = "all",
                string? search = null) =>
            {
                await using var db = await factory.CreateDbContextAsync();

                pageSize = Math.Clamp(pageSize, 10, 50);
                page = Math.Max(1, page);

                var query = db.TableSessions
                    .AsNoTracking()
                    .Include(s => s.Table)
                    .Include(s => s.CreatedBy)
                    .AsQueryable();

                // Time filter
                var now = DateTime.UtcNow;
                query = filter switch
                {
                    "today" => query.Where(s => s.OpenedAt >= now.Date && s.OpenedAt < now.Date.AddDays(1)),
                    "week"  => query.Where(s => s.OpenedAt >= now.AddDays(-7)),
                    _       => query
                };

                // Search by table number or cashier name
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var q = search.Trim().ToLower();
                    var stripped = q.StartsWith("table ") ? q["table ".Length..].Trim() : q;
                    var tableNum = int.TryParse(stripped, out int n) ? (int?)n : null;
                    query = query.Where(s =>
                        (tableNum.HasValue && s.Table.TableNumber == tableNum.Value) ||
                        s.CreatedBy.UserName.ToLower().Contains(q));
                }

                // Aggregate stats on the full filtered set before pagination
                var totalCount   = await query.CountAsync();
                var totalRevenue = await query.SumAsync(s => s.TotalAmount ?? 0m);
                var openCount    = await query.CountAsync(s => s.SessionStatus == SessionStatus.Open);
                var cashCount    = await query.CountAsync(s => s.PaymentMethod == PaymentMethod.Cash);
                var khqrCount    = await query.CountAsync(s => s.PaymentMethod == PaymentMethod.KHQR);

                var raw = await query
                    .OrderByDescending(s => s.OpenedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return Results.Ok(new SessionListResponse(
                    raw.Select(s => MapToResponse(s)).ToList(),
                    totalCount, totalRevenue, openCount, cashCount, khqrCount));
            }).RequireAuthorization("AdminOnly");

            // GET dashboard stats (Admin only)
            sessions.MapGet("/stats", async (
                [FromServices] IDbContextFactory<AppDbContext> factory) =>
            {
                await using var db = await factory.CreateDbContextAsync();

                var today = DateTime.UtcNow.Date;
                var tomorrow = today.AddDays(1);

                var todayRevenue = await db.TableSessions
                    .Where(s => s.SessionStatus == SessionStatus.Closed &&
                                s.ClosedAt >= today && s.ClosedAt < tomorrow)
                    .SumAsync(s => s.TotalAmount ?? 0m);

                var todayClosedCount = await db.TableSessions
                    .CountAsync(s => s.SessionStatus == SessionStatus.Closed &&
                                     s.ClosedAt >= today && s.ClosedAt < tomorrow);

                var openCount = await db.TableSessions
                    .CountAsync(s => s.SessionStatus == SessionStatus.Open);

                var topItemsRaw = await db.OrderItems
                    .GroupBy(i => i.MenuItemId)
                    .Select(g => new { MenuItemId = g.Key, TotalQty = g.Sum(i => i.Quantity) })
                    .OrderByDescending(x => x.TotalQty)
                    .Take(5)
                    .ToListAsync();

                var itemIds = topItemsRaw.Select(x => x.MenuItemId).ToList();
                var menuItems = await db.MenuItems
                    .Where(m => itemIds.Contains(m.Id))
                    .ToDictionaryAsync(m => m.Id, m => m.ItemName);

                var topItems = topItemsRaw
                    .Select(x => new TopItemResponse(
                        menuItems.GetValueOrDefault(x.MenuItemId, "Unknown"),
                        x.TotalQty))
                    .ToList();

                return Results.Ok(new SessionStatsResponse(
                    todayRevenue,
                    todayClosedCount,
                    openCount,
                    topItems));
            }).RequireAuthorization("AdminOnly");

            // GET session by id
            sessions.MapGet("/{id:int}", async (
                int id,
                [FromServices] IDbContextFactory<AppDbContext> factory) =>
            {
                await using var db = await factory.CreateDbContextAsync();
                var session = await db.TableSessions
                    .Include(s => s.Table)
                    .Include(s => s.CreatedBy)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (session is null) return Results.NotFound();

                return Results.Ok(MapToResponse(session));
            }).RequireAuthorization("CashierOnly");

            // GET active session by table id
            sessions.MapGet("/table/{tableId:int}/active", async (
                int tableId,
                [FromServices] IDbContextFactory<AppDbContext> factory) =>
            {
                await using var db = await factory.CreateDbContextAsync();
                var session = await db.TableSessions
                    .Include(s => s.Table)
                    .Include(s => s.CreatedBy)
                    .FirstOrDefaultAsync(s =>
                        s.TableId == tableId &&
                        s.SessionStatus == SessionStatus.Open);

                if (session is null) return Results.NotFound();

                return Results.Ok(MapToResponse(session));
            }).RequireAuthorization("CashierOnly");

            // GET today's stats for the logged-in cashier
            sessions.MapGet("/my-stats", async (
                [FromServices] IDbContextFactory<AppDbContext> factory,
                ClaimsPrincipal user) =>
            {
                await using var db = await factory.CreateDbContextAsync();

                var cashierId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
                var today = DateTime.UtcNow.Date;
                var tomorrow = today.AddDays(1);

                var myTodayRevenue = await db.TableSessions
                    .Where(s => s.CreatedById == cashierId &&
                                s.SessionStatus == SessionStatus.Closed &&
                                s.ClosedAt >= today && s.ClosedAt < tomorrow)
                    .SumAsync(s => s.TotalAmount ?? 0m);

                var myTodayCount = await db.TableSessions
                    .CountAsync(s => s.CreatedById == cashierId &&
                                     s.SessionStatus == SessionStatus.Closed &&
                                     s.ClosedAt >= today && s.ClosedAt < tomorrow);

                return Results.Ok(new
                {
                    TodayRevenue = myTodayRevenue,
                    TodayClosedSessions = myTodayCount
                });
            }).RequireAuthorization("CashierOnly");

            // GET session status — AllowAnonymous (used by client menu page)
            sessions.MapGet("/{id:int}/status", async (
                int id,
                [FromServices] IDbContextFactory<AppDbContext> factory) =>
            {
                await using var db = await factory.CreateDbContextAsync();
                var session = await db.TableSessions
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (session is null) return Results.NotFound();

                return Results.Ok(new { isOpen = session.SessionStatus == SessionStatus.Open });
            }).AllowAnonymous();

            // GET resolve a static table QR token to its current session — AllowAnonymous (customer scans table sticker)
            sessions.MapGet("/by-table-token/{token:guid}", async (
                Guid token,
                [FromServices] IDbContextFactory<AppDbContext> factory) =>
            {
                await using var db = await factory.CreateDbContextAsync();

                var table = await db.Tables
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.PublicToken == token);

                if (table is null) return Results.NotFound();

                var session = await db.TableSessions
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s =>
                        s.TableId == table.Id &&
                        s.SessionStatus == SessionStatus.Open);

                return Results.Ok(new TableTokenResolveResponse(
                    table.TableNumber,
                    session is not null,
                    session?.Id));
            }).AllowAnonymous();

            // POST open session — Cashier only
            sessions.MapPost("/", async (
                [FromBody] CreateSessionRequest request,
                [FromServices] IDbContextFactory<AppDbContext> factory,
                ClaimsPrincipal user) =>
            {

                await using var db = await factory.CreateDbContextAsync();

                // Check table exists
                var table = await db.Tables.FindAsync(request.TableId);
                if (table is null)
                    return Results.NotFound("Table not found.");

                // Check table is available
                if (table.TableStatus == TableStatus.Occupied)
                    return Results.Conflict("Table already has an open session.");

                // Check no existing open session
                var existingSession = await db.TableSessions
                    .AnyAsync(s => s.TableId == request.TableId &&
                                   s.SessionStatus == SessionStatus.Open);
                if (existingSession)
                    return Results.Conflict("Table already has an open session.");

                var cashierId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

                var session = new TableSession
                {
                    TableId = request.TableId,
                    CreatedById = cashierId,
                    OpenedAt = DateTime.UtcNow,
                    SessionStatus = SessionStatus.Open
                };

                // Mark table as occupied
                table.TableStatus = TableStatus.Occupied;
                db.Tables.Update(table);
                db.TableSessions.Add(session);
                try
                {
                    await db.SaveChangesAsync();
                }
                catch (Microsoft.EntityFrameworkCore.DbUpdateException)
                {
                    // Unique index violation — another request opened the same table concurrently
                    return Results.Conflict("Table already has an open session.");
                }

                // Reload with includes for response
                await db.Entry(session).Reference(s => s.Table).LoadAsync();
                await db.Entry(session).Reference(s => s.CreatedBy).LoadAsync();

                return Results.Created($"/api/sessions/{session.Id}", MapToResponse(session));
            }).RequireAuthorization("CashierOnly");

            // PATCH close session — Cashier only
            sessions.MapPatch("/{id:int}/close", async (
                int id,
                [FromBody] CloseSessionRequest request,
                [FromServices] IDbContextFactory<AppDbContext> factory,
                ClaimsPrincipal user) =>
            {
                await using var db = await factory.CreateDbContextAsync();

                var session = await db.TableSessions
                    .Include(s => s.Table)
                    .Include(s => s.CreatedBy)
                    .Include(s => s.Orders)
                        .ThenInclude(o => o.OrderItems)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (session is null) return Results.NotFound();
                if (session.SessionStatus == SessionStatus.Closed)
                    return Results.Conflict("Session is already closed.");

                var cashierId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (session.CreatedById != cashierId)
                    return Results.Forbid();

                if (!Enum.TryParse<PaymentMethod>(request.PaymentMethod, ignoreCase: true, out var paymentMethod))
                    return Results.BadRequest("Invalid payment method.");

                // Calculate total from all order items
                var total = session.Orders
                    .SelectMany(o => o.OrderItems)
                    .Sum(i => i.UnitPrice * i.Quantity);

                session.SessionStatus = SessionStatus.Closed;
                session.ClosedAt = DateTime.UtcNow;
                session.PaymentMethod = paymentMethod;
                session.TotalAmount = total;
                session.AmountReceived = request.AmountReceived;

                // Advance any Ready orders to Served — session close is the terminal event
                foreach (var order in session.Orders.Where(o => o.OrderStatus == OrderStatus.Ready))
                    order.OrderStatus = OrderStatus.Served;

                // Mark table as available again
                session.Table.TableStatus = TableStatus.Available;

                await db.SaveChangesAsync();

                return Results.Ok(MapToResponse(session));
            }).RequireAuthorization("CashierOnly");
        }

        private static SessionResponse MapToResponse(TableSession session) => new(
            session.Id,
            session.TableId,
            session.Table.TableNumber,
            session.SessionStatus.ToString(),
            session.PaymentMethod?.ToString(),
            session.TotalAmount,
            session.AmountReceived,
            session.OpenedAt,
            session.ClosedAt,
            session.CreatedById,
            session.CreatedBy?.UserName ?? string.Empty
        );
    }
}
