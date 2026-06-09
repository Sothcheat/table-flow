using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TableFlow.Api.Data.Entities;

namespace TableFlow.Api.Data
{
    public static class SeedData
    {
        public static async Task SeedAsync(AppDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            string[] roles = ["Admin", "Cashier", "Kitchen"];

            foreach (string role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            string adminEmail = "admin@tableflow.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Admin",
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(admin, "Admin@123");
                await userManager.AddToRoleAsync(admin, "Admin");
            }

            if (!await db.Categories.AnyAsync())
            {
                db.Categories.AddRange(
                    new Category { CategoryName = "Food", DisplayOrder = 1 },
                    new Category { CategoryName = "Drinks", DisplayOrder = 2 },
                    new Category { CategoryName = "Desserts", DisplayOrder = 3 }
                );
                await db.SaveChangesAsync();
            }

            if (!await db.Tables.AnyAsync())
            {
                for (int i = 1; i <= 5; i++)
                {
                    db.Tables.Add(new Table
                    {
                        TableNumber = i,
                        TableStatus = TableStatus.Available
                    });
                }
                await db.SaveChangesAsync();
            }
        }
    }
}
