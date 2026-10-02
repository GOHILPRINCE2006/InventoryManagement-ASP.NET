using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Controllers
{
    [Authorize]
    public class StockController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StockController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Stock
        public async Task<IActionResult> Index()
        {
            var vm = new StockDashboardViewModel
            {
                TotalProducts = await _context.Products.CountAsync(p => p.IsActive),
                LowStockCount = await _context.Products.CountAsync(p => p.IsActive && p.Quantity > 0 && p.Quantity <= p.MinStockLevel),
                OutOfStockCount = await _context.Products.CountAsync(p => p.IsActive && p.Quantity == 0),
                TotalStockValue = await _context.Products
                    .Where(p => p.IsActive)
                    .SumAsync(p => (decimal?)(p.Quantity * p.CostPrice)) ?? 0
            };

            return View(vm);
        }

        // GET: /Stock/History
        public async Task<IActionResult> History(string? type, int? productId)
        {
            var query = _context.StockTransactions
                .Include(t => t.Product)
                .Include(t => t.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(type) && (type == "IN" || type == "OUT"))
                query = query.Where(t => t.Type == type);

            if (productId.HasValue && productId.Value > 0)
                query = query.Where(t => t.ProductId == productId.Value);

            var transactions = await query
                .OrderByDescending(t => t.TransactionDate)
                .Take(200)
                .Select(t => new StockTransactionViewModel
                {
                    Id = t.Id,
                    ProductName = t.Product.Name,
                    ProductSKU = t.Product.SKU,
                    Type = t.Type,
                    Quantity = t.Quantity,
                    Reference = t.Reference,
                    Notes = t.Notes,
                    UserEmail = t.User.Email ?? "",
                    TransactionDate = t.TransactionDate
                })
                .ToListAsync();

            var vm = new StockHistoryViewModel
            {
                Transactions = transactions,
                FilterType = type,
                FilterProductId = productId,
                Products = await _context.Products
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.Name)
                    .Select(p => new DropdownItemVM { Id = p.Id, Name = p.Name })
                    .ToListAsync()
            };

            return View(vm);
        }

        // GET: /Stock/LowStock
        public async Task<IActionResult> LowStock()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Where(p => p.IsActive && p.Quantity <= p.MinStockLevel)
                .OrderBy(p => p.Quantity)
                .Select(p => new LowStockItemVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    SKU = p.SKU,
                    CategoryName = p.Category.Name,
                    SupplierName = p.Supplier != null ? p.Supplier.Name : "—",
                    Quantity = p.Quantity,
                    MinStockLevel = p.MinStockLevel,
                    Shortage = p.MinStockLevel - p.Quantity
                })
                .ToListAsync();

            return View(products);
        }
    }
}