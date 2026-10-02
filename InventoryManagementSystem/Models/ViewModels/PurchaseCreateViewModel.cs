using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventoryManagementSystem.Models.ViewModels
{
    public class PurchaseCreateViewModel
    {
        [Required]
        [Display(Name = "Supplier")]
        public int SupplierId { get; set; }

        [Required]
        [Display(Name = "Purchase Date")]
        public DateTime PurchaseDate { get; set; } = DateTime.Today;

        [StringLength(500)]
        public string? Notes { get; set; }

        public List<PurchaseLineItemViewModel> Items { get; set; } = new();

        // Dropdowns
        public IEnumerable<SelectListItem> Suppliers { get; set; } = new List<SelectListItem>();
        public IEnumerable<ProductOption> Products { get; set; } = new List<ProductOption>();
    }

    public class PurchaseLineItemViewModel
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
    }

    public class ProductOption
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal CostPrice { get; set; }
    }
}