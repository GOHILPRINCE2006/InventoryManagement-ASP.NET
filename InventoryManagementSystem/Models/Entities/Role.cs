using Microsoft.AspNetCore.Identity;

namespace InventoryManagementSystem.Models.Entities
{
    public class Role : IdentityRole
    {

        public string? Description { get; set; }

        public Role() : base() { }

        public Role(string roleName) : base(roleName) { }
    }
}