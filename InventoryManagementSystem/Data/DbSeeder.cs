using InventoryManagementSystem.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Apply pending migrations
            await context.Database.MigrateAsync();

            // ============ SEED ROLES ============
            string[] roles = { "Admin", "Manager", "Employee" };
            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new Role
                    {
                        Name = roleName,
                        Description = $"{roleName} role"
                    });
                }
            }

            // ============ SEED DEFAULT USERS ============
            await CreateUserIfNotExists(
                userManager,
                email: "admin@ims.com",
                password: "Admin@123",
                fullName: "System Administrator",
                role: "Admin");

            await CreateUserIfNotExists(
                userManager,
                email: "manager@ims.com",
                password: "Manager@123",
                fullName: "Default Manager",
                role: "Manager");

            await CreateUserIfNotExists(
                userManager,
                email: "employee@ims.com",
                password: "Employee@123",
                fullName: "Default Employee",
                role: "Employee");

            // ============ SEED CATEGORIES ============
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

        private static async Task CreateUserIfNotExists(
            UserManager<ApplicationUser> userManager,
            string email,
            string password,
            string fullName,
            string role)
        {
            var existing = await userManager.FindByEmailAsync(email);
            if (existing != null) return;

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = fullName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, role);
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Failed to create user {email}: {errors}");
            }
        }
    }
}