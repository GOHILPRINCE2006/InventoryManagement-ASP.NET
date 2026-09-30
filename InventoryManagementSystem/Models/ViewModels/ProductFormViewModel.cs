using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventoryManagementSystem.Models.ViewModels
{
    public class ProductFormViewModel
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(50)]
        [Display(Name = "SKU")]
        public string SKU { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required, Range(0.01, 9999999)]
        [Display(Name = "Selling Price")]
        public decimal Price { get; set; }

        [Required, Range(0.01, 9999999)]
        [Display(Name = "Cost Price")]
        public decimal CostPrice { get; set; }

        [Required, Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [Required, Range(0, int.MaxValue)]
        [Display(Name = "Minimum Stock Level")]
        public int MinStockLevel { get; set; } = 5;

        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Display(Name = "Supplier")]
        public int? SupplierId { get; set; }

        public bool IsActive { get; set; } = true;

        // Dropdowns
        public IEnumerable<SelectListItem> Categories { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Suppliers { get; set; } = new List<SelectListItem>();
    }
}