using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Models.ViewModels
{
    public class SupplierViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Supplier name is required.")]
        [StringLength(100, MinimumLength = 2)]
        [Display(Name = "Supplier Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Contact Person")]
        public string? ContactPerson { get; set; }

        [StringLength(20)]
        [Phone(ErrorMessage = "Enter a valid phone number.")]
        public string? Phone { get; set; }

        [StringLength(150)]
        [EmailAddress(ErrorMessage = "Enter a valid email.")]
        public string? Email { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }

        // For list view
        public int ProductCount { get; set; }
    }
}