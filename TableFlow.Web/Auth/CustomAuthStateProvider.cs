using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Identity;
using System.Runtime.InteropServices.JavaScript;
using System.Security.Claims;
using TableFlow.Api.Data.Entities;

namespace TableFlow.Web.Auth
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ProtectedLocalStorage _storage;
        private readonly UserManager<ApplicationUser> _userManager;

        private static readonly AuthenticationState Anonymous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        public CustomAuthStateProvider(ProtectedLocalStorage storage, UserManager<ApplicationUser> userManager)
        {
            _storage = storage;
            _userManager = userManager;
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

                var user = await _userManager.FindByIdAsync(result.Value);
                if (user == null) return Anonymous;

                var roles = await _userManager.GetRolesAsync(user);

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
            catch (InvalidOperationException)
            {
                return Anonymous;
            }
            catch (JSException)
            {
                return Anonymous;
            }
        }

        public async Task LoginAsync(string userId)
        {
            await _storage.SetAsync("userId", userId);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public async Task LogoutAsync()
        {
            await _storage.DeleteAsync("userId");
            NotifyAuthenticationStateChanged(Task.FromResult(Anonymous));
        }
    }
}
