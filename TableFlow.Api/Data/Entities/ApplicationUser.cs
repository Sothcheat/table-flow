using Microsoft.AspNetCore.Identity;

namespace TableFlow.Api.Data.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public List<TableSession> TableSessions { get; set; } = new();
    }
}
