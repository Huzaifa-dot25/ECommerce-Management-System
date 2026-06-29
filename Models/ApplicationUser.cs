using Microsoft.AspNetCore.Identity;

namespace E_com.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string? ProfilePicture { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public bool Status { get; set; } = true; // True for active, False for disabled

        // Navigation properties
        public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ShoppingCart? ShoppingCart { get; set; }
    }
}
