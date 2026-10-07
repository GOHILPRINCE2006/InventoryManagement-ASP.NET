using InventoryManagementSystem.Data;
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
    [Authorize]
    public class SalesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICurrentTenantService _tenantService;

        public SalesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ICurrentTenantService tenantService)
        {
            _context = context;
            _userManager = userManager;
            _tenantService = tenantService;
        }

        public async Task<IActionResult> Index()
        {
            var tenantId = _tenantService.TenantId ?? 0;

            var sales = await _context.Sales
                .Where(s => s.TenantId == tenantId)
                .Include(s => s.Customer)
                .Include(s => s.User)
                .Include(s => s.SaleItems)
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();

            return View(sales);
        }

        public async Task<IActionResult> Details(int id)
        {
            var tenantId = _tenantService.TenantId ?? 0;

            var sale = await _context.Sales
                .Where(s => s.Id == id && s.TenantId == tenantId)
                .Include(s => s.Customer)
                .Include(s => s.User)
                .Include(s => s.SaleItems)
                    .ThenInclude(si => si.Product)
                .FirstOrDefaultAsync();

            if (sale == null) return NotFound();
            return View(sale);
        }

        public async Task<IActionResult> Create()
        {
            var model = new SaleCreateViewModel();
            await PopulateDropdowns(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SaleCreateViewModel model)
        {
            ModelState.Remove(nameof(model.Items));

            if (model.Items == null || model.Items.Count == 0 || model.Items.All(i => i.ProductId == 0))
                ModelState.AddModelError("", "Please add at least one product.");

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(model);
                return View(model);
            }

            var validItems = model.Items?
                .Where(i => i.ProductId > 0 && i.Quantity > 0)
                .ToList() ?? new List<SaleLineItemViewModel>();

            if (!validItems.Any())
            {
                ModelState.AddModelError("", "Please add at least one valid product.");
                await PopulateDropdowns(model);
                return View(model);
            }

            var tenantId = _tenantService.TenantId ?? 0;

            foreach (var item in validItems)
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == item.ProductId && p.TenantId == tenantId);

                if (product == null)
                {
                    ModelState.AddModelError("", "Product not found.");
                    await PopulateDropdowns(model);
                    return View(model);
                }

                if (product.Quantity < item.Quantity)
                {
                    ModelState.AddModelError("", $"Insufficient stock for '{product.Name}'. Available: {product.Quantity}, Requested: {item.Quantity}.");
                }
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(model);
                return View(model);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                    return RedirectToAction("Login", "Account");

                var invoiceNumber = await GenerateInvoiceNumberAsync(tenantId);

                var sale = new Sale
                {
                    InvoiceNumber = invoiceNumber,
                    CustomerId = model.CustomerId,
                    SaleDate = model.SaleDate,
                    Notes = model.Notes,
                    Discount = model.Discount,
                    UserId = currentUser.Id,
                    TotalAmount = 0,
                    TenantId = tenantId
                };

                _context.Sales.Add(sale);
                await _context.SaveChangesAsync();

                decimal total = 0;

                foreach (var item in validItems)
                {
                    var product = await _context.Products
                        .FirstOrDefaultAsync(p => p.Id == item.ProductId && p.TenantId == tenantId);
                    if (product == null) continue;

                    var subTotal = item.Quantity * item.UnitPrice;
                    total += subTotal;

                    _context.SaleItems.Add(new SaleItem
                    {
                        SaleId = sale.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        SubTotal = subTotal
                    });

                    product.Quantity -= item.Quantity;
                    product.UpdatedAt = DateTime.UtcNow;

                    _context.StockTransactions.Add(new StockTransaction
                    {
                        ProductId = product.Id,
                        Type = "OUT",
                        Quantity = item.Quantity,
                        Reference = invoiceNumber,
                        Notes = "Sale to customer",
                        UserId = currentUser.Id,
                        TransactionDate = DateTime.UtcNow,
                        TenantId = tenantId
                    });
                }

                sale.TotalAmount = total - model.Discount;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = $"Sale {invoiceNumber} created. Stock updated.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", "Error: " + ex.Message);
                await PopulateDropdowns(model);
                return View(model);
            }
        }

        private async Task<string> GenerateInvoiceNumberAsync(int tenantId)
        {
            var year = DateTime.Now.Year;
            var count = await _context.Sales
                .CountAsync(s => s.TenantId == tenantId && s.SaleDate.Year == year) + 1;
            return $"INV-{year}-{count:D4}";
        }

        private async Task PopulateDropdowns(SaleCreateViewModel model)
        {
            var tenantId = _tenantService.TenantId ?? 0;

            model.Customers = await _context.Customers
                .Where(c => c.TenantId == tenantId)
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToListAsync();

            model.Products = await _context.Products
                .Where(p => p.TenantId == tenantId && p.IsActive && p.Quantity > 0)
                .OrderBy(p => p.Name)
                .Select(p => new ProductOption
                {
                    Id = p.Id,
                    Name = p.Name,
                    SKU = p.SKU,
                    CostPrice = p.Price
                })
                .ToListAsync();
        }
    }
}