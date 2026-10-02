namespace InventoryManagementSystem.Models.ViewModels
{
    public class StockDashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int LowStockCount { get; set; }
        public int OutOfStockCount { get; set; }
        public decimal TotalStockValue { get; set; }
    }

    public class StockHistoryViewModel
    {
        public List<StockTransactionViewModel> Transactions { get; set; } = new();
        public string? FilterType { get; set; }
        public int? FilterProductId { get; set; }
        public List<DropdownItemVM> Products { get; set; } = new();
    }

    public class StockTransactionViewModel
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSKU { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string? Reference { get; set; }
        public string? Notes { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
    }

    public class LowStockItemVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int MinStockLevel { get; set; }
        public int Shortage { get; set; }
    }

    public class DropdownItemVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}