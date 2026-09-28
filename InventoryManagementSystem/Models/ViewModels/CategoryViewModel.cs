using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Models.ViewModels
{
    public class CategoryViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(100, MinimumLength = 2)]
        [Display(Name = "Category Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Description { get; set; }

        // For list view — count of products in category
        public int ProductCount { get; set; }
    }
}