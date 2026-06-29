using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_com.Models
{
    public class CartItem
    {
        public int Id { get; set; }

        [Required]
        public int ShoppingCartId { get; set; }

        [ForeignKey("ShoppingCartId")]
        public virtual ShoppingCart? ShoppingCart { get; set; }

        [Required]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        [Required]
        [Range(1, 100)]
        public int Quantity { get; set; }
    }
}
