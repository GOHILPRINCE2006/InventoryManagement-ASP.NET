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
    public class PurchasesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICurrentTenantService _tenantService;

        public PurchasesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ICurrentTenantService tenantService)
        {
            _context = context;
            _userManager = userManager;
            _tenantService = tenantService;
        }

        public async Task<IActionResult> Index()
        {
            var tenantId = _tenantService.TenantId ?? 0;

            var purchases = await _context.Purchases
                .Where(p => p.TenantId == tenantId)
                .Include(p => p.Supplier)
                .Include(p => p.User)
                .Include(p => p.PurchaseItems)
                .OrderByDescending(p => p.PurchaseDate)
                .ToListAsync();

            return View(purchases);
        }

        public async Task<IActionResult> Details(int id)
        {
            var tenantId = _tenantService.TenantId ?? 0;

            var purchase = await _context.Purchases
                .Where(p => p.Id == id && p.TenantId == tenantId)
                .Include(p => p.Supplier)
                .Include(p => p.User)
                .Include(p => p.PurchaseItems)
                    .ThenInclude(pi => pi.Product)
                .FirstOrDefaultAsync();

            if (purchase == null) return NotFound();
            return View(purchase);
        }

        [Authorize(Roles = "Admin,Manager,Employee")]
        public async Task<IActionResult> Create()
        {
            var model = new PurchaseCreateViewModel();
            await PopulateDropdowns(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager,Employee")]
        public async Task<IActionResult> Create(PurchaseCreateViewModel model)
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
                .ToList() ?? new List<PurchaseLineItemViewModel>();

            if (!validItems.Any())
            {
                ModelState.AddModelError("", "Please add at least one valid product.");
                await PopulateDropdowns(model);
                return View(model);
            }

            var tenantId = _tenantService.TenantId ?? 0;

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                    return RedirectToAction("Login", "Account");

                var purchaseNumber = await GeneratePurchaseNumberAsync(tenantId);

                var purchase = new Purchase
                {
                    PurchaseNumber = purchaseNumber,
                    SupplierId = model.SupplierId,
                    PurchaseDate = model.PurchaseDate,
                    Notes = model.Notes,
                    UserId = currentUser.Id,
                    TotalAmount = 0,
                    TenantId = tenantId
                };

                _context.Purchases.Add(purchase);
                await _context.SaveChangesAsync();

                decimal total = 0;

                foreach (var item in validItems)
                {
                    var product = await _context.Products
                        .FirstOrDefaultAsync(p => p.Id == item.ProductId && p.TenantId == tenantId);
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

                    product.Quantity += item.Quantity;
                    product.UpdatedAt = DateTime.UtcNow;

                    _context.StockTransactions.Add(new StockTransaction
                    {
                        ProductId = product.Id,
                        Type = "IN",
                        Quantity = item.Quantity,
                        Reference = purchaseNumber,
                        Notes = "Purchase from supplier",
                        UserId = currentUser.Id,
                        TransactionDate = DateTime.UtcNow,
                        TenantId = tenantId
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

        private async Task<string> GeneratePurchaseNumberAsync(int tenantId)
        {
            var year = DateTime.Now.Year;
            var count = await _context.Purchases
                .CountAsync(p => p.TenantId == tenantId && p.PurchaseDate.Year == year) + 1;
            return $"PO-{year}-{count:D4}";
        }

        private async Task PopulateDropdowns(PurchaseCreateViewModel model)
        {
            var tenantId = _tenantService.TenantId ?? 0;

            model.Suppliers = await _context.Suppliers
                .Where(s => s.TenantId == tenantId)
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                .ToListAsync();

            model.Products = await _context.Products
                .Where(p => p.TenantId == tenantId && p.IsActive)
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