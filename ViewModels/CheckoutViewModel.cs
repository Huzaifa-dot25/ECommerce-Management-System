using System.ComponentModel.DataAnnotations;
using E_com.Models;

namespace E_com.ViewModels
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Street Address is required.")]
        [Display(Name = "Shipping Address")]
        public string StreetAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required.")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "State is required.")]
        public string State { get; set; } = string.Empty;

        [Required(ErrorMessage = "Postal Code is required.")]
        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone Number is required.")]
        [Phone(ErrorMessage = "Invalid phone number format.")]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; } = "Cash On Delivery";

        [Display(Name = "Coupon Code")]
        public string? CouponCode { get; set; }

        public ShoppingCart? Cart { get; set; }
    }
}
