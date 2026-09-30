using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models.Entities;
using InventoryManagementSystem.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Controllers
{
    [Authorize]
    public class SuppliersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SuppliersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Suppliers
        public async Task<IActionResult> Index()
        {
            var suppliers = await _context.Suppliers
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

        // GET: /Suppliers/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var supplier = await _context.Suppliers
                .Where(s => s.Id == id)
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

        // GET: /Suppliers/Create
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Suppliers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create(SupplierViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var supplier = new Supplier
            {
                Name = model.Name,
                ContactPerson = model.ContactPerson,
                Phone = model.Phone,
                Email = model.Email,
                Address = model.Address,
                CreatedAt = DateTime.UtcNow
            };

            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Supplier created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Suppliers/Edit/5
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null) return NotFound();

            var model = new SupplierViewModel
            {
                Id = supplier.Id,
                Name = supplier.Name,
                ContactPerson = supplier.ContactPerson,
                Phone = supplier.Phone,
                Email = supplier.Email,
                Address = supplier.Address
            };

            return View(model);
        }

        // POST: /Suppliers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id, SupplierViewModel model)
        {
            if (id != model.Id) return NotFound();
            if (!ModelState.IsValid) return View(model);

            var supplier = await _context.Suppliers.FindAsync(id);
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

        // GET: /Suppliers/Delete/5
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            var supplier = await _context.Suppliers
                .Where(s => s.Id == id)
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

        // POST: /Suppliers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var supplier = await _context.Suppliers
                .Include(s => s.Products)
                .FirstOrDefaultAsync(s => s.Id == id);

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