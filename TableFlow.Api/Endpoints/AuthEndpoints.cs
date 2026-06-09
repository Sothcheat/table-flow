using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using TableFlow.Api.Data.Entities;
using TableFlow.Api.DTOs;

namespace TableFlow.Api.Endpoints
{
    public static class AuthEndpoints
    {
        public static void MapAuthEndpoints(this WebApplication app)
        {
            app.MapPost("api/auth/login", async (LoginRequest req, UserManager<ApplicationUser> userManager, IConfiguration config) =>
            {
                var user = await userManager.FindByEmailAsync(req.Email);
                if (user == null)
                    return Results.Unauthorized();

                var passwordOk = await userManager.CheckPasswordAsync(user, req.Password);
                if (!passwordOk) return Results.Unauthorized();

                var roles = await userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault() ?? "";

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                    new Claim(ClaimTypes.Role, role),
                    new Claim("fullName", user.FullName)
                };

                var token = new JwtSecurityToken(
                        issuer: config["Jwt:Issuer"], 
                        audience: config["Jwt:Audience"], 
                        claims: claims, 
                        expires: DateTime.UtcNow.AddHours(8), 
                        signingCredentials: creds
                );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                return Results.Ok(new LoginResponse(
                    tokenString,
                    user.Id,
                    user.FullName,
                    user.Email!,
                    role
                ));
            });
        }
    }
}
