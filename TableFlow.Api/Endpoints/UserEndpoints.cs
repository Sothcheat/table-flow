using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TableFlow.Api.Data.Entities;
using TableFlow.Api.DTOs;

namespace TableFlow.Api.Endpoints
{
    public static class UserEndpoints
    {
        // Get all non-admin user
        public static void MapUserEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/users").RequireAuthorization("AdminOnly");

            group.MapGet("/", async (UserManager<ApplicationUser> userManager) => {
                var users = userManager.Users.ToList();

                var result = new List<UserResponse>();

                foreach (var user in users)
                {
                    var roles = await userManager.GetRolesAsync(user);
                    var role = roles.FirstOrDefault() ?? "";


                    if (role == "Admin") continue;

                    result.Add(new UserResponse(user.Id, user.FullName, user.Email!, role));
                }
                return Results.Ok(result);
            });


            // post - create new user
            group.MapPost("/", async (CreateUserRequest req, UserManager<ApplicationUser> userManager) =>
            {
                if (req.Role != "Cashier" && req.Role != "Kitchen")
                {
                    return Results.BadRequest("Role must be Cashier or Kitchen.");
                }

                var existing = await userManager.FindByEmailAsync(req.Email);
                if (existing != null)
                {
                    return Results.Conflict("Email already register.");
                }

                var user = new ApplicationUser
                {
                    FullName = req.Fullname,
                    UserName = req.Email,
                    Email = req.Email,
                    EmailConfirmed = true
                };

                var createReasult = await userManager.CreateAsync(user, req.Password);
                if(!createReasult.Succeeded)
                {
                    return Results.BadRequest(createReasult.Errors.Select(e => e.Description));
                }

                await userManager.AddToRoleAsync(user, req.Role);

                return Results.Created($"/api/users/{user.Id}", new UserResponse(user.Id, user.FullName, user.Email!, req.Role));
            });

            // put - update user
            group.MapPut("/{id}", async (string id, UpdateUserRequest req, UserManager<ApplicationUser> userManager) =>
            {
                var user = await userManager.FindByIdAsync(id);
                if (user == null) return Results.NotFound();

                if (req.Role != "Cashier" && req.Role != "Kitchen")
                    return Results.BadRequest("Role must be Cashier or Kitchen.");

                user.FullName = req.Fullname;
                user.UserName = req.Email;
                user.Email = req.Email;
                user.NormalizedEmail = req.Email.ToUpperInvariant();
                user.NormalizedUserName = req.Email.ToUpperInvariant();

                var updatedResult = await userManager.UpdateAsync(user);

                if (!updatedResult.Succeeded)
                    return Results.BadRequest(updatedResult.Errors.Select(e => e.Description));

                var currentRoles = await userManager.GetRolesAsync(user);
                await userManager.RemoveFromRolesAsync(user, currentRoles);
                await userManager.AddToRoleAsync(user, req.Role);

                return Results.Ok(new UserResponse(user.Id, user.FullName, user.Email!, req.Role));
            });

            // delete - delete user
            group.MapDelete("/{id}", async (
            string id,
            UserManager<ApplicationUser> userManager) =>
            {
                var user = await userManager.FindByIdAsync(id);
                if (user == null) return Results.NotFound();

                var roles = await userManager.GetRolesAsync(user);
                if (roles.Contains("Admin"))
                    return Results.BadRequest("Cannot delete admin accounts.");

                await userManager.DeleteAsync(user);
                return Results.NoContent();
            });
        }




    }
}
