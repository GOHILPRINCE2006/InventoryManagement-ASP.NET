using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventoryManagementSystem.Models.ViewModels
{
    public class SaleCreateViewModel
    {
        [Required]
        [Display(Name = "Customer")]
        public int CustomerId { get; set; }

        [Required]
        [Display(Name = "Sale Date")]
        public DateTime SaleDate { get; set; } = DateTime.Today;

        [Range(0, 9999999)]
        public decimal Discount { get; set; } = 0;

        [StringLength(500)]
        public string? Notes { get; set; }

        public List<SaleLineItemViewModel> Items { get; set; } = new();

        public IEnumerable<SelectListItem> Customers { get; set; } = new List<SelectListItem>();
        public IEnumerable<ProductOption> Products { get; set; } = new List<ProductOption>();
    }

    public class SaleLineItemViewModel
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
    }
}