using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagementSystem.Models.Entities
{
    public class Product
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string SKU { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CostPrice { get; set; }

        public int Quantity { get; set; }

        public int MinStockLevel { get; set; } = 5;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // ⭐ NEW: Tenant relationship
        public int TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;

        // Existing relationships
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public int? SupplierId { get; set; }
        public Supplier? Supplier { get; set; }

        // Existing navigation properties
        public ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();
        public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
        public ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();
    }
}