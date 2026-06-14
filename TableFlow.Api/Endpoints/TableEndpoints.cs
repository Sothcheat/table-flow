using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TableFlow.Api.Data;
using TableFlow.Api.Data.Entities;
using TableFlow.Api.DTOs;

namespace TableFlow.Api.Endpoints
{
    public static class TableEndpoints
    {
        public static void MapTableEndpoints(this WebApplication app)
        {
            var tables = app.MapGroup("/api/tables").RequireAuthorization();

            // GET all tables — Admin + Cashier
            tables.MapGet("/", async ([FromServices] IDbContextFactory<AppDbContext> factory) =>
            {
                await using var db = await factory.CreateDbContextAsync();
                var tables = await db.Tables.OrderBy(t => t.TableNumber).ToListAsync();
                var response = tables.Select(t => new TableResponse(
                    t.Id,
                    t.TableNumber,
                    t.TableStatus.ToString()
                )).ToList();
                return Results.Ok(response);
            });

            // POST create table — Admin only
            tables.MapPost("/", async ([FromServices] IDbContextFactory<AppDbContext> factory,
                [FromBody] CreateTableRequest request) =>
            {
                await using var db = await factory.CreateDbContextAsync();

                var exists = await db.Tables.AnyAsync(t => t.TableNumber == request.TableNumber);
                if (exists)
                    return Results.Conflict($"Table {request.TableNumber} already exists.");

                var table = new Table { TableNumber = request.TableNumber };
                db.Tables.Add(table);
                await db.SaveChangesAsync();

                return Results.Created($"/api/tables/{table.Id}",
                    new TableResponse(table.Id, table.TableNumber, table.TableStatus.ToString()));
            }).RequireAuthorization("AdminOnly");

            // PUT update table number — Admin only
            tables.MapPut("/{id:int}", async ([FromServices] IDbContextFactory<AppDbContext> factory,
                int id, [FromBody] UpdateTableRequest request) =>
            {
                await using var db = await factory.CreateDbContextAsync();

                var table = await db.Tables.FindAsync(id);
                if (table is null)
                    return Results.NotFound();

                var exists = await db.Tables.AnyAsync(t => t.TableNumber == request.TableNumber && t.Id != id);
                if (exists)
                    return Results.Conflict($"Table {request.TableNumber} already exists.");

                table.TableNumber = request.TableNumber;
                await db.SaveChangesAsync();

                return Results.Ok(new TableResponse(table.Id, table.TableNumber, table.TableStatus.ToString()));
            }).RequireAuthorization("AdminOnly");

            // DELETE table — Admin only
            tables.MapDelete("/{id:int}", async ([FromServices] IDbContextFactory<AppDbContext> factory, int id) =>
            {
                await using var db = await factory.CreateDbContextAsync();

                var table = await db.Tables.FindAsync(id);
                if (table is null)
                    return Results.NotFound();

                db.Tables.Remove(table);
                await db.SaveChangesAsync();
                return Results.NoContent();
            }).RequireAuthorization("AdminOnly");

            // PATCH status — Cashier only
            tables.MapPatch("/{id:int}/status", async ([FromServices] IDbContextFactory<AppDbContext> factory,
                int id, [FromBody] UpdateTableStatusRequest request) =>
            {
                await using var db = await factory.CreateDbContextAsync();

                var table = await db.Tables.FindAsync(id);
                if (table is null)
                    return Results.NotFound();

                if (!Enum.TryParse<TableStatus>(request.Status, ignoreCase: true, out var newStatus))
                    return Results.BadRequest("Invalid status value.");

                table.TableStatus = newStatus;
                await db.SaveChangesAsync();

                return Results.Ok(new TableResponse(table.Id, table.TableNumber, table.TableStatus.ToString()));
            }).RequireAuthorization("CashierOnly");
        }
    }
}
