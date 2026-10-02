using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models.Entities;
using InventoryManagementSystem.Models.ViewModels;
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

        public SalesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Sales
        public async Task<IActionResult> Index()
        {
            var sales = await _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.User)
                .Include(s => s.SaleItems)
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();

            return View(sales);
        }

        // GET: /Sales/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var sale = await _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.User)
                .Include(s => s.SaleItems)
                    .ThenInclude(si => si.Product)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sale == null) return NotFound();
            return View(sale);
        }

        // GET: /Sales/Create
        public async Task<IActionResult> Create()
        {
            var model = new SaleCreateViewModel();
            await PopulateDropdowns(model);
            return View(model);
        }

        // POST: /Sales/Create
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

            var validItems = model.Items.Where(i => i.ProductId > 0 && i.Quantity > 0).ToList();

            if (!validItems.Any())
            {
                ModelState.AddModelError("", "Please add at least one valid product.");
                await PopulateDropdowns(model);
                return View(model);
            }

            // ============ STOCK VALIDATION ============
            foreach (var item in validItems)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null)
                {
                    ModelState.AddModelError("", $"Product not found.");
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

                var invoiceNumber = await GenerateInvoiceNumberAsync();

                var sale = new Sale
                {
                    InvoiceNumber = invoiceNumber,
                    CustomerId = model.CustomerId,
                    SaleDate = model.SaleDate,
                    Notes = model.Notes,
                    Discount = model.Discount,
                    UserId = currentUser.Id,
                    TotalAmount = 0
                };

                _context.Sales.Add(sale);
                await _context.SaveChangesAsync();

                decimal total = 0;

                foreach (var item in validItems)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
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

                    // ✅ Stock decreases
                    product.Quantity -= item.Quantity;
                    product.UpdatedAt = DateTime.UtcNow;

                    _context.StockTransactions.Add(new StockTransaction
                    {
                        ProductId = product.Id,
                        Type = "OUT",
                        Quantity = item.Quantity,
                        Reference = invoiceNumber,
                        Notes = $"Sale to customer",
                        UserId = currentUser.Id,
                        TransactionDate = DateTime.UtcNow
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

        private async Task<string> GenerateInvoiceNumberAsync()
        {
            var year = DateTime.Now.Year;
            var count = await _context.Sales
                .CountAsync(s => s.SaleDate.Year == year) + 1;
            return $"INV-{year}-{count:D4}";
        }

        private async Task PopulateDropdowns(SaleCreateViewModel model)
        {
            model.Customers = await _context.Customers
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToListAsync();

            model.Products = await _context.Products
                .Where(p => p.IsActive && p.Quantity > 0)
                .OrderBy(p => p.Name)
                .Select(p => new ProductOption
                {
                    Id = p.Id,
                    Name = p.Name,
                    SKU = p.SKU,
                    CostPrice = p.Price  // For sales, we use Price as default
                })
                .ToListAsync();
        }
    }
}