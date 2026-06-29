using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using E_com.Data;
using E_com.Interfaces;
using E_com.Models;
using E_com.ViewModels;

namespace E_com.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public CheckoutController(
            ICartService cartService, 
            IOrderService orderService, 
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _cartService = cartService;
            _orderService = orderService;
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            var cart = await _cartService.GetCartByUserIdAsync(userId);
            if (!cart.CartItems.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty. Add products before checkout.";
                return RedirectToAction("Index", "Cart");
            }

            // Fetch default address if available
            var defaultAddress = await _context.Addresses.FirstOrDefaultAsync(a => a.UserId == userId && a.IsDefault);

            var model = new CheckoutViewModel
            {
                Cart = cart,
                StreetAddress = defaultAddress?.StreetAddress ?? string.Empty,
                City = defaultAddress?.City ?? string.Empty,
                State = defaultAddress?.State ?? string.Empty,
                PostalCode = defaultAddress?.PostalCode ?? string.Empty,
                Phone = defaultAddress?.Phone ?? string.Empty
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(CheckoutViewModel model)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            var cart = await _cartService.GetCartByUserIdAsync(userId);
            if (!cart.CartItems.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Create address object
                    var address = new Address
                    {
                        UserId = userId,
                        StreetAddress = model.StreetAddress,
                        City = model.City,
                        State = model.State,
                        PostalCode = model.PostalCode,
                        Phone = model.Phone,
                        IsDefault = false
                    };

                    // Check if user already has an address, if not make this default
                    var hasAddress = await _context.Addresses.AnyAsync(a => a.UserId == userId);
                    if (!hasAddress)
                    {
                        address.IsDefault = true;
                    }
                    
                    // Save address for future checkout convenience
                    _context.Addresses.Add(address);
                    await _context.SaveChangesAsync();

                    // Place the order
                    var order = await _orderService.CreateOrderAsync(userId, address, model.PaymentMethod, model.CouponCode);

                    TempData["SuccessMessage"] = "Order placed successfully!";
                    return RedirectToAction("OrderConfirmation", new { orderId = order.Id });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Order placement failed: " + ex.Message);
                }
            }

            model.Cart = cart;
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> OrderConfirmation(int orderId)
        {
            var userId = _userManager.GetUserId(User);
            var order = await _orderService.GetOrderByIdAsync(orderId);
            
            if (order == null || order.UserId != userId)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpGet]
        public async Task<IActionResult> ValidateCoupon(string code, decimal subtotal)
        {
            var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.CouponCode == code && c.Status && c.ExpiryDate >= DateTime.UtcNow);
            if (coupon == null)
            {
                return Json(new { success = false, message = "Invalid or expired coupon code." });
            }

            decimal discountAmount = subtotal * (coupon.DiscountPercentage / 100);
            decimal total = subtotal - discountAmount;

            return Json(new { 
                success = true, 
                discountPercent = coupon.DiscountPercentage, 
                discountAmount = discountAmount.ToString("F2"), 
                total = total.ToString("F2") 
            });
        }
    }
}
