using Microsoft.EntityFrameworkCore;
using E_com.Data;
using E_com.Interfaces;
using E_com.Models;

namespace E_com.Services
{
    public class WishlistService : IWishlistService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICartService _cartService;

        public WishlistService(ApplicationDbContext context, ICartService cartService)
        {
            _context = context;
            _cartService = cartService;
        }

        public async Task<IEnumerable<Wishlist>> GetWishlistByUserIdAsync(string userId)
        {
            return await _context.Wishlists
                .Include(w => w.Product)
                .ThenInclude(p => p!.ProductImages)
                .Where(w => w.UserId == userId)
                .ToListAsync();
        }

        public async Task AddToWishlistAsync(string userId, int productId)
        {
            var alreadyExists = await IsInWishlistAsync(userId, productId);
            if (!alreadyExists)
            {
                var item = new Wishlist
                {
                    UserId = userId,
                    ProductId = productId,
                    AddedDate = DateTime.UtcNow
                };
                _context.Wishlists.Add(item);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveFromWishlistAsync(string userId, int wishlistId)
        {
            var item = await _context.Wishlists.FirstOrDefaultAsync(w => w.Id == wishlistId && w.UserId == userId);
            if (item != null)
            {
                _context.Wishlists.Remove(item);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsInWishlistAsync(string userId, int productId)
        {
            return await _context.Wishlists.AnyAsync(w => w.UserId == userId && w.ProductId == productId);
        }

        public async Task MoveToCartAsync(string userId, int wishlistId)
        {
            var wishlist = await _context.Wishlists.FirstOrDefaultAsync(w => w.Id == wishlistId && w.UserId == userId);
            if (wishlist != null)
            {
                // Add to cart
                await _cartService.AddToCartAsync(userId, wishlist.ProductId, 1);
                
                // Remove from wishlist
                _context.Wishlists.Remove(wishlist);
                await _context.SaveChangesAsync();
            }
        }
    }
}
