using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Models.Entities
{
    public class Category
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ⭐ NEW: Tenant relationship
        public int TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;

        // Existing navigation property
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}