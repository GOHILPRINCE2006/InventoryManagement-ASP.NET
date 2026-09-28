using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models.Entities;
using InventoryManagementSystem.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Controllers
{
    [Authorize]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Categories
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
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

        // GET: /Categories/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var category = await _context.Categories
                .Where(c => c.Id == id)
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

        // GET: /Categories/Create
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create(CategoryViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Check duplicate name
            var exists = await _context.Categories
                .AnyAsync(c => c.Name.ToLower() == model.Name.ToLower());

            if (exists)
            {
                ModelState.AddModelError(nameof(model.Name), "A category with this name already exists.");
                return View(model);
            }

            var category = new Category
            {
                Name = model.Name,
                Description = model.Description,
                CreatedAt = DateTime.UtcNow
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Category created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Categories/Edit/5
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _context.Categories.FindAsync(id);
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

        // POST: /Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id, CategoryViewModel model)
        {
            if (id != model.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(model);

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound();

            // Duplicate check (excluding self)
            var exists = await _context.Categories
                .AnyAsync(c => c.Id != id && c.Name.ToLower() == model.Name.ToLower());

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

        // GET: /Categories/Delete/5
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories
                .Where(c => c.Id == id)
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

        // POST: /Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

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