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
    public class PurchasesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PurchasesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Purchases
        public async Task<IActionResult> Index()
        {
            var purchases = await _context.Purchases
                .Include(p => p.Supplier)
                .Include(p => p.User)
                .Include(p => p.PurchaseItems)
                .OrderByDescending(p => p.PurchaseDate)
                .ToListAsync();

            return View(purchases);
        }

        // GET: /Purchases/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var purchase = await _context.Purchases
                .Include(p => p.Supplier)
                .Include(p => p.User)
                .Include(p => p.PurchaseItems)
                    .ThenInclude(pi => pi.Product)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (purchase == null) return NotFound();
            return View(purchase);
        }

        // GET: /Purchases/Create
        [Authorize(Roles = "Admin,Manager,Employee")]
        public async Task<IActionResult> Create()
        {
            var model = new PurchaseCreateViewModel();
            await PopulateDropdowns(model);
            return View(model);
        }

        // POST: /Purchases/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager,Employee")]
        public async Task<IActionResult> Create(PurchaseCreateViewModel model)
        {
            // Remove Items validation from ModelState (we validate manually)
            ModelState.Remove(nameof(model.Items));

            if (model.Items == null || model.Items.Count == 0 || model.Items.All(i => i.ProductId == 0))
            {
                ModelState.AddModelError("", "Please add at least one product.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(model);
                return View(model);
            }

            // Filter out empty rows
            var validItems = model.Items.Where(i => i.ProductId > 0 && i.Quantity > 0).ToList();

            if (!validItems.Any())
            {
                ModelState.AddModelError("", "Please add at least one valid product.");
                await PopulateDropdowns(model);
                return View(model);
            }

            // Begin DB transaction
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                    return RedirectToAction("Login", "Account");

                var purchaseNumber = await GeneratePurchaseNumberAsync();

                var purchase = new Purchase
                {
                    PurchaseNumber = purchaseNumber,
                    SupplierId = model.SupplierId,
                    PurchaseDate = model.PurchaseDate,
                    Notes = model.Notes,
                    UserId = currentUser.Id,
                    TotalAmount = 0
                };

                _context.Purchases.Add(purchase);
                await _context.SaveChangesAsync(); // Get purchase Id

                decimal total = 0;

                foreach (var item in validItems)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product == null) continue;

                    var subTotal = item.Quantity * item.UnitPrice;
                    total += subTotal;

                    _context.PurchaseItems.Add(new PurchaseItem
                    {
                        PurchaseId = purchase.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        SubTotal = subTotal
                    });

                    // ✅ Stock increases
                    product.Quantity += item.Quantity;
                    product.UpdatedAt = DateTime.UtcNow;

                    // Log stock transaction
                    _context.StockTransactions.Add(new StockTransaction
                    {
                        ProductId = product.Id,
                        Type = "IN",
                        Quantity = item.Quantity,
                        Reference = purchaseNumber,
                        Notes = $"Purchase from supplier",
                        UserId = currentUser.Id,
                        TransactionDate = DateTime.UtcNow
                    });
                }

                purchase.TotalAmount = total;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = $"Purchase {purchaseNumber} created. Stock updated.";
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

        private async Task<string> GeneratePurchaseNumberAsync()
        {
            var year = DateTime.Now.Year;
            var count = await _context.Purchases
                .CountAsync(p => p.PurchaseDate.Year == year) + 1;
            return $"PO-{year}-{count:D4}";
        }

        private async Task PopulateDropdowns(PurchaseCreateViewModel model)
        {
            model.Suppliers = await _context.Suppliers
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                .ToListAsync();

            model.Products = await _context.Products
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .Select(p => new ProductOption
                {
                    Id = p.Id,
                    Name = p.Name,
                    SKU = p.SKU,
                    CostPrice = p.CostPrice
                })
                .ToListAsync();
        }
    }
}