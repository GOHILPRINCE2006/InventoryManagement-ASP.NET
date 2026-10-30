using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models.ViewModels;
using InventoryManagementSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentTenantService _tenantService;

        public HomeController(ApplicationDbContext context, ICurrentTenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
        }

        public async Task<IActionResult> Index()
        {
            if (!(User.Identity?.IsAuthenticated ?? false))
                return View(new DashboardViewModel());

            var tenantId = _tenantService.TenantId ?? 0;

            //Load tenant info for Admin
            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId);
            ViewBag.ShopName = tenant?.Name ?? "—";
            ViewBag.ShopCode = tenant?.Code ?? "—";
            ViewBag.IsAdmin = User.IsInRole("Admin");

            var vm = new DashboardViewModel
            {
                TotalProducts = await _context.Products.CountAsync(p => p.TenantId == tenantId && p.IsActive),
                TotalCategories = await _context.Categories.CountAsync(c => c.TenantId == tenantId),
                TotalSuppliers = await _context.Suppliers.CountAsync(s => s.TenantId == tenantId),
                TotalCustomers = await _context.Customers.CountAsync(c => c.TenantId == tenantId),
                TotalUsers = await _context.Users.CountAsync(u => u.TenantId == tenantId),
                TotalPurchases = await _context.Purchases.CountAsync(p => p.TenantId == tenantId),
                TotalSales = await _context.Sales.CountAsync(s => s.TenantId == tenantId),
                TotalRevenue = await _context.Sales.Where(s => s.TenantId == tenantId).SumAsync(s => (decimal?)s.TotalAmount) ?? 0,
                TotalPurchaseCost = await _context.Purchases.Where(p => p.TenantId == tenantId).SumAsync(p => (decimal?)p.TotalAmount) ?? 0,
                LowStockCount = await _context.Products.CountAsync(p => p.TenantId == tenantId && p.IsActive && p.Quantity <= p.MinStockLevel),

                LowStockItems = await _context.Products
                    .Where(p => p.TenantId == tenantId && p.IsActive && p.Quantity <= p.MinStockLevel)
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
                    .Where(s => s.TenantId == tenantId)
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