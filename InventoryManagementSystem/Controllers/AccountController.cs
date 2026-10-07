using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models.Entities;
using InventoryManagementSystem.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace InventoryManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        // GET: /Account/Login
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null || !user.IsActive)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                // Ensure TenantId claim exists
                var claims = await _userManager.GetClaimsAsync(user);
                if (!claims.Any(c => c.Type == "TenantId"))
                {
                    await _userManager.AddClaimAsync(user,
                        new Claim("TenantId", user.TenantId.ToString()));

                    await _signInManager.RefreshSignInAsync(user);
                }

                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    return Redirect(model.ReturnUrl);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Invalid email or password.");
            return View(model);
        }

        // GET: /Account/Register
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View(new RegisterViewModel());
        }

        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Check email uniqueness
            var existing = await _userManager.FindByEmailAsync(model.Email);
            if (existing != null)
            {
                ModelState.AddModelError(nameof(model.Email), "This email is already registered.");
                return View(model);
            }

            int tenantId;
            string roleName;

            // ============ FLOW 1: Create new shop ============
            if (model.AccountType == "CreateShop")
            {
                if (string.IsNullOrWhiteSpace(model.ShopName))
                {
                    ModelState.AddModelError(nameof(model.ShopName), "Shop Name is required.");
                    return View(model);
                }

                // Generate unique shop code from shop name
                var shopCode = await GenerateUniqueShopCodeAsync(model.ShopName);

                var newTenant = new Tenant
                {
                    Name = model.ShopName.Trim(),
                    Code = shopCode,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Tenants.Add(newTenant);
                await _context.SaveChangesAsync();

                tenantId = newTenant.Id;
                roleName = "Admin";  // Creator becomes Admin
            }
            // ============ FLOW 2: Join existing shop ============
            else
            {
                if (string.IsNullOrWhiteSpace(model.ShopCode))
                {
                    ModelState.AddModelError(nameof(model.ShopCode), "Shop Code is required.");
                    return View(model);
                }

                var tenant = await _context.Tenants
                    .FirstOrDefaultAsync(t => t.Code.ToLower() == model.ShopCode.Trim().ToLower() && t.IsActive);

                if (tenant == null)
                {
                    ModelState.AddModelError(nameof(model.ShopCode), "No active shop found with this code.");
                    return View(model);
                }

                tenantId = tenant.Id;
                roleName = "Employee";  // Joiner becomes Employee
            }

            // Create the user
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                TenantId = tenantId
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, roleName);

                await _userManager.AddClaimAsync(user,
                    new Claim("TenantId", user.TenantId.ToString()));

                await _signInManager.SignInAsync(user, isPersistent: false);

                TempData["Success"] = model.AccountType == "CreateShop"
                    ? $"Welcome! Your shop '{model.ShopName}' has been created."
                    : $"Welcome! You've joined the shop successfully.";

                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // GET: /Account/Profile
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            var roles = await _userManager.GetRolesAsync(user);
            var tenant = await _context.Tenants.FindAsync(user.TenantId);

            var model = new ProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                RoleName = roles.FirstOrDefault() ?? "",
                CreatedAt = user.CreatedAt
            };

            ViewBag.TenantName = tenant?.Name ?? "—";
            ViewBag.TenantCode = tenant?.Code ?? "—";

            return View(model);
        }

        // POST: /Account/Profile
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            if (!ModelState.IsValid)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var tenant = await _context.Tenants.FindAsync(user.TenantId);
                model.Email = user.Email ?? "";
                model.RoleName = roles.FirstOrDefault() ?? "";
                model.CreatedAt = user.CreatedAt;
                ViewBag.TenantName = tenant?.Name ?? "—";
                ViewBag.TenantCode = tenant?.Code ?? "—";
                return View(model);
            }

            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.Address = model.Address;

            await _userManager.UpdateAsync(user);

            TempData["Success"] = "Profile updated successfully.";
            return RedirectToAction(nameof(Profile));
        }

        // ============ HELPER: Generate unique shop code ============
        private async Task<string> GenerateUniqueShopCodeAsync(string shopName)
        {
            // Take first 6 letters, uppercase, remove non-alphanumeric
            var baseCode = Regex.Replace(shopName.ToUpper(), "[^A-Z0-9]", "");
            if (baseCode.Length > 8) baseCode = baseCode.Substring(0, 8);
            if (string.IsNullOrEmpty(baseCode)) baseCode = "SHOP";

            // Try base code first
            var code = baseCode;
            var counter = 1;

            while (await _context.Tenants.AnyAsync(t => t.Code == code))
            {
                counter++;
                code = $"{baseCode}{counter}";
            }

            return code;
        }
    }
}