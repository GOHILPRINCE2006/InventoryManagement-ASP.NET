using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (!(User.Identity?.IsAuthenticated ?? false))
                return View(new DashboardViewModel());

            var vm = new DashboardViewModel
            {
                TotalProducts = await _context.Products.CountAsync(p => p.IsActive),
                TotalCategories = await _context.Categories.CountAsync(),
                TotalSuppliers = await _context.Suppliers.CountAsync(),
                TotalCustomers = await _context.Customers.CountAsync(),
                TotalUsers = await _context.Users.CountAsync(),
                TotalPurchases = await _context.Purchases.CountAsync(),
                TotalSales = await _context.Sales.CountAsync(),
                TotalRevenue = await _context.Sales.SumAsync(s => (decimal?)s.TotalAmount) ?? 0,
                TotalPurchaseCost = await _context.Purchases.SumAsync(p => (decimal?)p.TotalAmount) ?? 0,
                LowStockCount = await _context.Products.CountAsync(p => p.IsActive && p.Quantity <= p.MinStockLevel),

                LowStockItems = await _context.Products
                    .Where(p => p.IsActive && p.Quantity <= p.MinStockLevel)
                    .OrderBy(p => p.Quantity)
                    .Take(5)
                    .Select(p => new LowStockItem
                    {
                        ProductId = p.Id,
                        ProductName = p.Name,
                        SKU = p.SKU,
                        Quantity = p.Quantity,
                        MinStockLevel = p.MinStockLevel
                    })
                    .ToListAsync(),

                RecentSales = await _context.Sales
                    .Include(s => s.Customer)
                    .OrderByDescending(s => s.SaleDate)
                    .Take(5)
                    .Select(s => new RecentSaleItem
                    {
                        SaleId = s.Id,
                        InvoiceNumber = s.InvoiceNumber,
                        CustomerName = s.Customer.Name,
                        TotalAmount = s.TotalAmount,
                        SaleDate = s.SaleDate
                    })
                    .ToListAsync()
            };

            return View(vm);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}