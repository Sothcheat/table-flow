using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TableFlow.Api.Data;
using TableFlow.Api.Data.Entities;
using TableFlow.Api.DTOs;
using QRCoder;

namespace TableFlow.Api.Endpoints
{
    public static class SessionEndpoints
    {
        public static void MapSessionEndpoints(this WebApplication app)
        {
            var sessions = app.MapGroup("/api/sessions").RequireAuthorization();

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

            // POST open session — Cashier only
            sessions.MapPost("/", async (
                [FromBody] CreateSessionRequest request,
                [FromServices] IDbContextFactory<AppDbContext> factory,
                ClaimsPrincipal user, [FromServices] IConfiguration config) =>
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
                await db.SaveChangesAsync();

                string? qrCodeBase64 = null;
                try
                {
                    var webBaseUrl = config["WebBaseUrl"] ?? "http://localhost:5010";
                    var menuUrl = $"{webBaseUrl}/menu?sessionId={session.Id}";
                    qrCodeBase64 = GenerateQrCode(menuUrl);
                }
                catch (Exception ex)
                {
                }

                // Reload with includes for response
                await db.Entry(session).Reference(s => s.Table).LoadAsync();
                await db.Entry(session).Reference(s => s.CreatedBy).LoadAsync();

                return Results.Created($"/api/sessions/{session.Id}", MapToResponse(session, qrCodeBase64));
            }).RequireAuthorization("CashierOnly");

            // PATCH close session — Cashier only
            sessions.MapPatch("/{id:int}/close", async (
                int id,
                [FromBody] CloseSessionRequest request,
                [FromServices] IDbContextFactory<AppDbContext> factory) =>
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

                // Mark table as available again
                session.Table.TableStatus = TableStatus.Available;

                await db.SaveChangesAsync();

                return Results.Ok(MapToResponse(session));
            }).RequireAuthorization("CashierOnly");
        }

        private static SessionResponse MapToResponse(TableSession session, string? qrCodeBase64 = null) => new(
            session.Id,
            session.TableId,
            session.Table.TableNumber,
            session.SessionStatus.ToString(),
            session.PaymentMethod?.ToString(),
            session.TotalAmount,
            session.OpenedAt,
            session.ClosedAt,
            session.CreatedById,
            session.CreatedBy?.UserName ?? string.Empty,
            qrCodeBase64
        );

        private static string GenerateQrCode(string url)
        {
            using var qrCodeData = QRCodeGenerator.GenerateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            byte[] qrCodeBytes = qrCode.GetGraphic(10);
            return Convert.ToBase64String(qrCodeBytes);
        }
    }
}
