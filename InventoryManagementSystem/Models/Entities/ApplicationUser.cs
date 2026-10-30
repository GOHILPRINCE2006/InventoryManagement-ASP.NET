using Microsoft.AspNetCore.Identity;

namespace InventoryManagementSystem.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        //  Tenant relationship
        public int TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;

        // Existing navigation properties
        public ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
        public ICollection<Sale> Sales { get; set; } = new List<Sale>();
        public ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();
    }
}