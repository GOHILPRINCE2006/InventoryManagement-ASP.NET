using InventoryManagementSystem.Models.Entities;
using InventoryManagementSystem.Models.ViewModels;
using InventoryManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly ICurrentTenantService _tenantService;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            RoleManager<Role> roleManager,
            ICurrentTenantService tenantService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _tenantService = tenantService;
        }

        public async Task<IActionResult> Index()
        {
            var tenantId = _tenantService.TenantId ?? 0;

            var users = await _userManager.Users
                .Where(u => u.TenantId == tenantId)
                .OrderBy(u => u.Email)
                .ToListAsync();

            var list = new List<UserListItemViewModel>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                list.Add(new UserListItemViewModel
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email ?? "",
                    RoleName = roles.FirstOrDefault() ?? "—",
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt
                });
            }

            return View(list);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var tenantId = _tenantService.TenantId ?? 0;

            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == id && u.TenantId == tenantId);

            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles.ToListAsync();

            var model = new UserEditViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? "",
                IsActive = user.IsActive,
                RoleName = roles.FirstOrDefault() ?? "Employee",
                AvailableRoles = allRoles.Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name,
                    Selected = r.Name == (roles.FirstOrDefault() ?? "")
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, UserEditViewModel model)
        {
            if (id != model.Id) return NotFound();
            if (!ModelState.IsValid) return View(model);

            var tenantId = _tenantService.TenantId ?? 0;

            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == id && u.TenantId == tenantId);

            if (user == null) return NotFound();

            user.FullName = model.FullName;
            user.IsActive = model.IsActive;

            await _userManager.UpdateAsync(user);

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (!currentRoles.Contains(model.RoleName))
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, model.RoleName);
            }

            TempData["Success"] = "User updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(string id)
        {
            var tenantId = _tenantService.TenantId ?? 0;

            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == id && u.TenantId == tenantId);

            if (user == null) return NotFound();

            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);

            TempData["Success"] = "User status updated.";
            return RedirectToAction(nameof(Index));
        }
    }
}