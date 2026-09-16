using InventoryManagementSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Apply any pending migrations
            await context.Database.MigrateAsync();

            // ============ SEED ROLES ============
            if (!await context.Roles.AnyAsync())
            {
                context.Roles.AddRange(
                    new Role { Name = "Admin", Description = "Full system access" },
                    new Role { Name = "Manager", Description = "Manages inventory, products, purchases, sales" },
                    new Role { Name = "Employee", Description = "Handles sales and purchases" }
                );
                await context.SaveChangesAsync();
            }

            // ============ SEED DEFAULT ADMIN USER ============
            if (!await context.Users.AnyAsync(u => u.Email == "admin@ims.com"))
            {
                var adminRole = await context.Roles.FirstAsync(r => r.Name == "Admin");

                var admin = new ApplicationUser
                {
                    FullName = "System Administrator",
                    Email = "admin@ims.com",
                    Phone = "0000000000",
                    Address = "Head Office",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    RoleId = adminRole.Id
                };

                // Use built-in password hasher
                var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<ApplicationUser>();
                admin.PasswordHash = hasher.HashPassword(admin, "Admin@123");

                context.Users.Add(admin);
                await context.SaveChangesAsync();
            }

            // ============ SEED SAMPLE CATEGORIES ============
            if (!await context.Categories.AnyAsync())
            {
                context.Categories.AddRange(
                    new Category { Name = "Electronics", Description = "Electronic devices and gadgets" },
                    new Category { Name = "Furniture", Description = "Office and home furniture" },
                    new Category { Name = "Stationery", Description = "Office and school supplies" },
                    new Category { Name = "Groceries", Description = "Food and daily essentials" }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}