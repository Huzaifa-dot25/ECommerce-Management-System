using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using E_com.Interfaces;
using E_com.Models;

namespace E_com.Controllers
{
    [Authorize]
    public class WishlistController : Controller
    {
        private readonly IWishlistService _wishlistService;
        private readonly UserManager<ApplicationUser> _userManager;

        public WishlistController(IWishlistService wishlistService, UserManager<ApplicationUser> userManager)
        {
            _wishlistService = wishlistService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            var wishlist = await _wishlistService.GetWishlistByUserIdAsync(userId);
            return View(wishlist);
        }

        [HttpPost]
        public async Task<IActionResult> Add(int productId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            await _wishlistService.AddToWishlistAsync(userId, productId);
            TempData["SuccessMessage"] = "Product added to wishlist!";
            
            // Redirect back to referring page if local, else Shop index
            string? referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer) && Url.IsLocalUrl(referer))
            {
                return Redirect(referer);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Remove(int wishlistId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            await _wishlistService.RemoveFromWishlistAsync(userId, wishlistId);
            TempData["SuccessMessage"] = "Item removed from wishlist!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> MoveToCart(int wishlistId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            await _wishlistService.MoveToCartAsync(userId, wishlistId);
            TempData["SuccessMessage"] = "Item moved to cart!";
            return RedirectToAction("Index", "Cart");
        }
    }
}
