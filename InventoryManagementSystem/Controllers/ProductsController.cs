using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models.Entities;
using InventoryManagementSystem.Models.ViewModels;
using InventoryManagementSystem.Services;
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
        private readonly ICurrentTenantService _tenantService;

        public ProductsController(ApplicationDbContext context, ICurrentTenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
        }

        public async Task<IActionResult> Index(string? search, int? categoryId, string? stockFilter)
        {
            var tenantId = _tenantService.TenantId ?? 0;

            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Where(p => p.TenantId == tenantId && p.IsActive)
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
                    .Where(c => c.TenantId == tenantId)
                    .Select(c => new CategoryOption { Id = c.Id, Name = c.Name })
                    .OrderBy(c => c.Name)
                    .ToListAsync()
            };

            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var tenantId = _tenantService.TenantId ?? 0;

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId);

            if (product == null) return NotFound();
            return View(product);
        }

        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create()
        {
            var model = new ProductFormViewModel();
            await PopulateDropdowns(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create(ProductFormViewModel model)
        {
            var tenantId = _tenantService.TenantId ?? 0;

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(model);
                return View(model);
            }

            var exists = await _context.Products
                .AnyAsync(p => p.TenantId == tenantId && p.SKU.ToLower() == model.SKU.ToLower());

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
                CreatedAt = DateTime.UtcNow,
                TenantId = tenantId
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Product created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id)
        {
            var tenantId = _tenantService.TenantId ?? 0;

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId);

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id, ProductFormViewModel model)
        {
            if (id != model.Id) return NotFound();

            var tenantId = _tenantService.TenantId ?? 0;

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(model);
                return View(model);
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId);

            if (product == null) return NotFound();

            var exists = await _context.Products
                .AnyAsync(p => p.TenantId == tenantId && p.Id != id && p.SKU.ToLower() == model.SKU.ToLower());

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

        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            var tenantId = _tenantService.TenantId ?? 0;

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId);

            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tenantId = _tenantService.TenantId ?? 0;

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId);

            if (product == null) return NotFound();

            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Product deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropdowns(ProductFormViewModel model)
        {
            var tenantId = _tenantService.TenantId ?? 0;

            model.Categories = await _context.Categories
                .Where(c => c.TenantId == tenantId)
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToListAsync();

            model.Suppliers = await _context.Suppliers
                .Where(s => s.TenantId == tenantId)
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                .ToListAsync();
        }
    }
}