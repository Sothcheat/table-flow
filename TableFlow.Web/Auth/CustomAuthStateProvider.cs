using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using System.Security.Claims;
using TableFlow.Api.Data;
using TableFlow.Api.Data.Entities;

namespace TableFlow.Web.Auth
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ProtectedSessionStorage _storage;
        private readonly IDbContextFactory<AppDbContext> _dbFactory;

        private static readonly AuthenticationState Anonymous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        public CustomAuthStateProvider(ProtectedSessionStorage storage, IDbContextFactory<AppDbContext> dbFactory)
        {
            _storage = storage;
            _dbFactory = dbFactory;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var result = await _storage.GetAsync<string>("userId");

                if (!result.Success || string.IsNullOrEmpty(result.Value)) 
                {
                    return Anonymous;
                }

                await using var db = await _dbFactory.CreateDbContextAsync();

                var user = await db.Users.FirstOrDefaultAsync(u => u.Id == result.Value);
                if (user == null) return Anonymous;

                var roles = await db.UserRoles
                    .Where(ur => ur.UserId == user.Id)
                    .Join(db.Roles,
                        ur => ur.RoleId,
                        r => r.Id,
                        (ur, r) => r.Name!)
                    .ToListAsync();

                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, user.Id),
                    new(ClaimTypes.Name, user.UserName ?? string.Empty),
                    new(ClaimTypes.Email, user.Email ?? string.Empty)

                };

                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                var identity = new ClaimsIdentity(claims, "tableflow");
                return new AuthenticationState(new ClaimsPrincipal(identity));
            } 
            catch (TaskCanceledException)
            {
                return Anonymous;
            }
            catch (InvalidOperationException)
            {
                return Anonymous;
            }
            catch (JSDisconnectedException)
            {
                return Anonymous;
            }
            catch (JSException)
            {
                return Anonymous;
            }
        }

        public async Task LoginAsync(string userId, string token)
        {
            await _storage.SetAsync("userId", userId);
            await _storage.SetAsync("authToken", token);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public async Task LogoutAsync()
        {
            try
            {
                await _storage.DeleteAsync("userId");
                await _storage.DeleteAsync("authToken");
            }
            catch (TaskCanceledException) { }
            catch (InvalidOperationException) { }
            catch (JSDisconnectedException) { }
            catch (JSException) { }

            try
            {
                NotifyAuthenticationStateChanged(Task.FromResult(Anonymous));
            }
            catch (ObjectDisposedException) { }
        }

        public async Task<string?> GetTokenAsync()
        {
            try
            {
                var result = await _storage.GetAsync<string>("authToken");
                return result.Success ? result.Value : null;
            }
            catch (JSDisconnectedException) { return null; }
            catch (JSException) { return null; }
            catch (TaskCanceledException) { return null; }
            catch (InvalidOperationException) { return null; }
        }
    }
}
