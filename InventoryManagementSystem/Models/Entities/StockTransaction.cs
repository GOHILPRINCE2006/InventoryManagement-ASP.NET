using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Models.Entities
{
    public class StockTransaction
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        [Required, StringLength(10)]
        public string Type { get; set; } = string.Empty; 

        public int Quantity { get; set; } 

        [StringLength(100)]
        public string? Reference { get; set; } 

        [StringLength(255)]
        public string? Notes { get; set; }

        public int UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;

        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    }
}