namespace InventoryManagementSystem.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int TotalCategories { get; set; }
        public int TotalSuppliers { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalUsers { get; set; }

        public int TotalPurchases { get; set; }
        public int TotalSales { get; set; }

        public decimal TotalRevenue { get; set; }
        public decimal TotalPurchaseCost { get; set; }
        public decimal EstimatedProfit => TotalRevenue - TotalPurchaseCost;

        public int LowStockCount { get; set; }
        public List<LowStockItem> LowStockItems { get; set; } = new();

        public List<RecentSaleItem> RecentSales { get; set; } = new();
        public List<RecentPurchaseItem> RecentPurchases { get; set; } = new();
    }


    public class LowStockItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int MinStockLevel { get; set; }
    }

    public class RecentSaleItem
    {
        public int SaleId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime SaleDate { get; set; }
    }

    public class RecentPurchaseItem
    {
        public int PurchaseId { get; set; }
        public string PurchaseNumber { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime PurchaseDate { get; set; }
    }
}