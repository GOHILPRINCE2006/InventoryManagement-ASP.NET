using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Models.ViewModels
{
    public class CustomerViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Customer name is required.")]
        [StringLength(100, MinimumLength = 2)]
        [Display(Name = "Customer Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(20)]
        [Phone(ErrorMessage = "Enter a valid phone number.")]
        public string? Phone { get; set; }

        [StringLength(150)]
        [EmailAddress(ErrorMessage = "Enter a valid email.")]
        public string? Email { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }

        public int SaleCount { get; set; }
    }
}