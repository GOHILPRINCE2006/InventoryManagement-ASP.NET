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
    public class SuppliersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentTenantService _tenantService;

        public SuppliersController(ApplicationDbContext context, ICurrentTenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
        }

        public async Task<IActionResult> Index()
        {
            var tenantId = _tenantService.TenantId ?? 0;

            var suppliers = await _context.Suppliers
                .Where(s => s.TenantId == tenantId)
                .Select(s => new SupplierViewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    ContactPerson = s.ContactPerson,
                    Phone = s.Phone,
                    Email = s.Email,
                    Address = s.Address,
                    ProductCount = s.Products.Count()
                })
                .OrderBy(s => s.Name)
                .ToListAsync();

            return View(suppliers);
        }

        public async Task<IActionResult> Details(int id)
        {
            var tenantId = _tenantService.TenantId ?? 0;

            var supplier = await _context.Suppliers
                .Where(s => s.Id == id && s.TenantId == tenantId)
                .Select(s => new SupplierViewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    ContactPerson = s.ContactPerson,
                    Phone = s.Phone,
                    Email = s.Email,
                    Address = s.Address,
                    ProductCount = s.Products.Count()
                })
                .FirstOrDefaultAsync();

            if (supplier == null) return NotFound();
            return View(supplier);
        }

        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create(SupplierViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var tenantId = _tenantService.TenantId ?? 0;

            var supplier = new Supplier
            {
                Name = model.Name,
                ContactPerson = model.ContactPerson,
                Phone = model.Phone,
                Email = model.Email,
                Address = model.Address,
                CreatedAt = DateTime.UtcNow,
                TenantId = tenantId
            };

            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Supplier created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id)
        {
            var tenantId = _tenantService.TenantId ?? 0;

            var supplier = await _context.Suppliers
                .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId);

            if (supplier == null) return NotFound();

            return View(new SupplierViewModel
            {
                Id = supplier.Id,
                Name = supplier.Name,
                ContactPerson = supplier.ContactPerson,
                Phone = supplier.Phone,
                Email = supplier.Email,
                Address = supplier.Address
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id, SupplierViewModel model)
        {
            if (id != model.Id) return NotFound();
            if (!ModelState.IsValid) return View(model);

            var tenantId = _tenantService.TenantId ?? 0;

            var supplier = await _context.Suppliers
                .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId);

            if (supplier == null) return NotFound();

            supplier.Name = model.Name;
            supplier.ContactPerson = model.ContactPerson;
            supplier.Phone = model.Phone;
            supplier.Email = model.Email;
            supplier.Address = model.Address;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Supplier updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            var tenantId = _tenantService.TenantId ?? 0;

            var supplier = await _context.Suppliers
                .Where(s => s.Id == id && s.TenantId == tenantId)
                .Select(s => new SupplierViewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    ContactPerson = s.ContactPerson,
                    Phone = s.Phone,
                    Email = s.Email,
                    Address = s.Address,
                    ProductCount = s.Products.Count()
                })
                .FirstOrDefaultAsync();

            if (supplier == null) return NotFound();
            return View(supplier);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tenantId = _tenantService.TenantId ?? 0;

            var supplier = await _context.Suppliers
                .Include(s => s.Products)
                .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId);

            if (supplier == null) return NotFound();

            if (supplier.Products.Any())
            {
                TempData["Error"] = "Cannot delete supplier — they are linked to products.";
                return RedirectToAction(nameof(Index));
            }

            _context.Suppliers.Remove(supplier);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Supplier deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}