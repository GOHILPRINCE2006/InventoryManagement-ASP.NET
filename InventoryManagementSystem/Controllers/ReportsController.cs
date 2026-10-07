using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models.ViewModels;
using InventoryManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentTenantService _tenantService;

        public ReportsController(ApplicationDbContext context, ICurrentTenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Sales(DateTime? from, DateTime? to, int? customerId)
        {
            var tenantId = _tenantService.TenantId ?? 0;

            var fromDate = from ?? DateTime.Today.AddDays(-30);
            var toDate = to ?? DateTime.Today;

            var query = _context.Sales
    .Include(s => s.Customer)
    .Include(s => s.User)
    .Include(s => s.SaleItems).ThenInclude(si => si.Product)
    .Where(s => s.TenantId == tenantId
        && s.SaleDate >= fromDate
        && s.SaleDate <= toDate.AddDays(1))
    .AsQueryable();

            if (customerId.HasValue && customerId.Value > 0)
                query = query.Where(s => s.CustomerId == customerId.Value);

            var sales = await query.OrderByDescending(s => s.SaleDate).ToListAsync();

            var rows = new List<SalesReportRow>();
            foreach (var sale in sales)
            {
                foreach (var item in sale.SaleItems)
                {
                    rows.Add(new SalesReportRow
                    {
                        InvoiceNumber = sale.InvoiceNumber,
                        SaleDate = sale.SaleDate,
                        CustomerName = sale.Customer.Name,
                        ProductName = item.Product.Name,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        SubTotal = item.SubTotal,
                        SoldBy = sale.User.Email ?? ""
                    });
                }
            }

            var vm = new ReportViewModel
            {
                FromDate = fromDate,
                ToDate = toDate,
                ReportType = "Sales",
                SalesRows = rows,
                TotalSalesAmount = sales.Sum(s => s.TotalAmount),
                TotalSalesCount = sales.Count,
                Customers = await _context.Customers
                    .Where(c => c.TenantId == tenantId)
                    .OrderBy(c => c.Name)
                    .Select(c => new DropdownItem { Id = c.Id, Name = c.Name })
                    .ToListAsync()
            };

            return View(vm);
        }

        public async Task<IActionResult> Inventory(int? categoryId)
        {
            var tenantId = _tenantService.TenantId ?? 0;

            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Where(p => p.TenantId == tenantId && p.IsActive);

            if (categoryId.HasValue && categoryId.Value > 0)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            var products = await query.OrderBy(p => p.Name).ToListAsync();

            var rows = products.Select(p => new InventoryReportRow
            {
                ProductName = p.Name,
                SKU = p.SKU,
                CategoryName = p.Category.Name,
                SupplierName = p.Supplier?.Name ?? "—",
                Quantity = p.Quantity,
                CostPrice = p.CostPrice,
                Price = p.Price,
                IsLowStock = p.Quantity <= p.MinStockLevel
            }).ToList();

            var vm = new ReportViewModel
            {
                ReportType = "Inventory",
                InventoryRows = rows,
                TotalInventoryItems = products.Count,
                TotalInventoryValue = products.Sum(p => p.Quantity * p.CostPrice),
                Categories = await _context.Categories
                    .Where(c => c.TenantId == tenantId)
                    .OrderBy(c => c.Name)
                    .Select(c => new DropdownItem { Id = c.Id, Name = c.Name })
                    .ToListAsync(),
                LowStockRows = products
                    .Where(p => p.Quantity <= p.MinStockLevel)
                    .Select(p => new LowStockReportRow
                    {
                        ProductId = p.Id,
                        ProductName = p.Name,
                        Quantity = p.Quantity,
                        MinStockLevel = p.MinStockLevel
                    }).ToList()
            };

            return View(vm);
        }
    }
}