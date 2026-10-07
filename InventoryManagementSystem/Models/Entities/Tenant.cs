using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Models.Entities
{
    public class Tenant
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string Code { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Address { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
        public ICollection<Category> Categories { get; set; } = new List<Category>();
        public ICollection<Supplier> Suppliers { get; set; } = new List<Supplier>();
        public ICollection<Customer> Customers { get; set; } = new List<Customer>();
        public ICollection<Product> Products { get; set; } = new List<Product>();
        public ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
        public ICollection<Sale> Sales { get; set; } = new List<Sale>();
        public ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();
    }
}