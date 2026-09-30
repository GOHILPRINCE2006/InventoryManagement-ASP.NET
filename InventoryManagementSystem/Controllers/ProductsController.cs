using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models.Entities;
using InventoryManagementSystem.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Products?search=&categoryId=&stockFilter=
        public async Task<IActionResult> Index(string? search, int? categoryId, string? stockFilter)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Where(p => p.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(search) ||
                    p.SKU.ToLower().Contains(search));
            }

            if (categoryId.HasValue && categoryId.Value > 0)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            // Stock filters are applied AFTER retrieving (because of computed properties)
            var products = await query
                .OrderBy(p => p.Name)
                .Select(p => new ProductListItem
                {
                    Id = p.Id,
                    Name = p.Name,
                    SKU = p.SKU,
                    CategoryName = p.Category.Name,
                    SupplierName = p.Supplier != null ? p.Supplier.Name : "—",
                    Price = p.Price,
                    Quantity = p.Quantity,
                    MinStockLevel = p.MinStockLevel
                })
                .ToListAsync();

            // Apply stock filter in memory
            if (!string.IsNullOrEmpty(stockFilter))
            {
                products = stockFilter switch
                {
                    "low" => products.Where(p => p.IsLowStock).ToList(),
                    "out" => products.Where(p => p.IsOutOfStock).ToList(),
                    _ => products
                };
            }

            var model = new ProductListViewModel
            {
                Products = products,
                Search = search,
                CategoryId = categoryId,
                StockFilter = stockFilter,
                Categories = await _context.Categories
                    .Select(c => new CategoryOption { Id = c.Id, Name = c.Name })
                    .OrderBy(c => c.Name)
                    .ToListAsync()
            };

            return View(model);
        }

        // GET: /Products/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();
            return View(product);
        }

        // GET: /Products/Create
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create()
        {
            var model = new ProductFormViewModel();
            await PopulateDropdowns(model);
            return View(model);
        }

        // POST: /Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create(ProductFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(model);
                return View(model);
            }

            // Duplicate SKU check
            var exists = await _context.Products.AnyAsync(p => p.SKU.ToLower() == model.SKU.ToLower());
            if (exists)
            {
                ModelState.AddModelError(nameof(model.SKU), "This SKU already exists.");
                await PopulateDropdowns(model);
                return View(model);
            }

            var product = new Product
            {
                Name = model.Name,
                SKU = model.SKU,
                Description = model.Description,
                Price = model.Price,
                CostPrice = model.CostPrice,
                Quantity = model.Quantity,
                MinStockLevel = model.MinStockLevel,
                CategoryId = model.CategoryId,
                SupplierId = model.SupplierId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Product created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Products/Edit/5
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            var model = new ProductFormViewModel
            {
                Id = product.Id,
                Name = product.Name,
                SKU = product.SKU,
                Description = product.Description,
                Price = product.Price,
                CostPrice = product.CostPrice,
                Quantity = product.Quantity,
                MinStockLevel = product.MinStockLevel,
                CategoryId = product.CategoryId,
                SupplierId = product.SupplierId,
                IsActive = product.IsActive
            };

            await PopulateDropdowns(model);
            return View(model);
        }

        // POST: /Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id, ProductFormViewModel model)
        {
            if (id != model.Id) return NotFound();
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(model);
                return View(model);
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            // Duplicate SKU check
            var exists = await _context.Products.AnyAsync(p => p.Id != id && p.SKU.ToLower() == model.SKU.ToLower());
            if (exists)
            {
                ModelState.AddModelError(nameof(model.SKU), "This SKU already exists.");
                await PopulateDropdowns(model);
                return View(model);
            }

            product.Name = model.Name;
            product.SKU = model.SKU;
            product.Description = model.Description;
            product.Price = model.Price;
            product.CostPrice = model.CostPrice;
            product.Quantity = model.Quantity;
            product.MinStockLevel = model.MinStockLevel;
            product.CategoryId = model.CategoryId;
            product.SupplierId = model.SupplierId;
            product.IsActive = model.IsActive;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Product updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Products/Delete/5
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();
            return View(product);
        }

        // POST: /Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            // Soft delete
            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Product deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        // Helper
        private async Task PopulateDropdowns(ProductFormViewModel model)
        {
            model.Categories = await _context.Categories
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToListAsync();

            model.Suppliers = await _context.Suppliers
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                .ToListAsync();
        }
    }
}