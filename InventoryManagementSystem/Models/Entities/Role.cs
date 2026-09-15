using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Models.Entities
{
    public class Role
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Name { get; set; } = string.Empty; 

        [StringLength(255)]
        public string? Description { get; set; }

       
        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    }
}