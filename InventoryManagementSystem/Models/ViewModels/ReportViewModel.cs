namespace InventoryManagementSystem.Models.ViewModels
{
    public class ReportViewModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? CategoryId { get; set; }
        public int? CustomerId { get; set; }
        public int? SupplierId { get; set; }

        public string ReportType { get; set; } = "Sales"; 

        public List<SalesReportRow> SalesRows { get; set; } = new();
        public decimal TotalSalesAmount { get; set; }
        public int TotalSalesCount { get; set; }

        public List<InventoryReportRow> InventoryRows { get; set; } = new();
        public int TotalInventoryItems { get; set; }
        public decimal TotalInventoryValue { get; set; }

        public List<LowStockReportRow> LowStockRows { get; set; } = new();

        public List<DropdownItem> Categories { get; set; } = new();
        public List<DropdownItem> Customers { get; set; } = new();
        public List<DropdownItem> Suppliers { get; set; } = new();
    }


    public class SalesReportRow
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime SaleDate { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal SubTotal { get; set; }
        public string SoldBy { get; set; } = string.Empty;
    }

    public class InventoryReportRow
    {
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal CostPrice { get; set; }
        public decimal Price { get; set; }
        public decimal StockValue => Quantity * CostPrice;
        public bool IsLowStock { get; set; }
    }

    public class LowStockReportRow
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int MinStockLevel { get; set; }
        public int Shortage => MinStockLevel - Quantity;
    }

    public class DropdownItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}