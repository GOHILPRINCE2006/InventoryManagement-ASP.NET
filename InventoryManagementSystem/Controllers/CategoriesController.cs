using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models.Entities;
using InventoryManagementSystem.Models.ViewModels;
using InventoryManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Controllers
{
    [Authorize]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentTenantService _tenantService;

        public CategoriesController(ApplicationDbContext context, ICurrentTenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
        }

        public async Task<IActionResult> Index()
        {
            var tenantId = _tenantService.TenantId ?? 0;

            var categories = await _context.Categories
                .Where(c => c.TenantId == tenantId)
                .Select(c => new CategoryViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    ProductCount = c.Products.Count()
                })
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(categories);
        }

        public async Task<IActionResult> Details(int id)
        {
            var tenantId = _tenantService.TenantId ?? 0;

            var category = await _context.Categories
                .Where(c => c.Id == id && c.TenantId == tenantId)
                .Select(c => new CategoryViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    ProductCount = c.Products.Count()
                })
                .FirstOrDefaultAsync();

            if (category == null)
                return NotFound();

            return View(category);
        }

        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create(CategoryViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var tenantId = _tenantService.TenantId ?? 0;

            var exists = await _context.Categories
                .AnyAsync(c => c.TenantId == tenantId && c.Name.ToLower() == model.Name.ToLower());

            if (exists)
            {
                ModelState.AddModelError(nameof(model.Name), "A category with this name already exists.");
                return View(model);
            }

            var category = new Category
            {
                Name = model.Name,
                Description = model.Description,
                CreatedAt = DateTime.UtcNow,
                TenantId = tenantId
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Category created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id)
        {
            var tenantId = _tenantService.TenantId ?? 0;

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId);

            if (category == null)
                return NotFound();

            var model = new CategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id, CategoryViewModel model)
        {
            if (id != model.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(model);

            var tenantId = _tenantService.TenantId ?? 0;

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId);

            if (category == null)
                return NotFound();

            var exists = await _context.Categories
                .AnyAsync(c => c.TenantId == tenantId && c.Id != id && c.Name.ToLower() == model.Name.ToLower());

            if (exists)
            {
                ModelState.AddModelError(nameof(model.Name), "A category with this name already exists.");
                return View(model);
            }

            category.Name = model.Name;
            category.Description = model.Description;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Category updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            var tenantId = _tenantService.TenantId ?? 0;

            var category = await _context.Categories
                .Where(c => c.Id == id && c.TenantId == tenantId)
                .Select(c => new CategoryViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    ProductCount = c.Products.Count()
                })
                .FirstOrDefaultAsync();

            if (category == null)
                return NotFound();

            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tenantId = _tenantService.TenantId ?? 0;

            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId);

            if (category == null)
                return NotFound();

            if (category.Products.Any())
            {
                TempData["Error"] = "Cannot delete category — it has products attached.";
                return RedirectToAction(nameof(Index));
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Category deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}