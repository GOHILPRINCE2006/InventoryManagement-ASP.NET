namespace InventoryManagementSystem.Models.ViewModels
{
    public class ProductListViewModel
    {
        public List<ProductListItem> Products { get; set; } = new();
        public string? Search { get; set; }
        public int? CategoryId { get; set; }
        public string? StockFilter { get; set; }  // all, low, out

        // For dropdown
        public List<CategoryOption> Categories { get; set; } = new();
    }

    public class ProductListItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int MinStockLevel { get; set; }
        public bool IsLowStock => Quantity > 0 && Quantity <= MinStockLevel;
        public bool IsOutOfStock => Quantity == 0;
    }

    public class CategoryOption
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}