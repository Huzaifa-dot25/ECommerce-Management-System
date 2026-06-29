using Microsoft.EntityFrameworkCore;
using E_com.Data;
using E_com.Interfaces;
using E_com.Models;

namespace E_com.Services
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;

        public CartService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ShoppingCart> GetCartByUserIdAsync(string userId)
        {
            var cart = await _context.ShoppingCarts
                .Include(sc => sc.CartItems)
                .ThenInclude(ci => ci.Product)
                .ThenInclude(p => p!.ProductImages)
                .FirstOrDefaultAsync(sc => sc.UserId == userId);

            if (cart == null)
            {
                cart = new ShoppingCart
                {
                    UserId = userId,
                    CreatedDate = DateTime.UtcNow
                };
                _context.ShoppingCarts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        public async Task AddToCartAsync(string userId, int productId, int quantity)
        {
            var cart = await GetCartByUserIdAsync(userId);
            var product = await _context.Products.FindAsync(productId);
            if (product == null || product.StockQuantity < quantity) return;

            var existingItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (existingItem != null)
            {
                int newQty = existingItem.Quantity + quantity;
                if (newQty <= product.StockQuantity)
                {
                    existingItem.Quantity = newQty;
                }
            }
            else
            {
                cart.CartItems.Add(new CartItem
                {
                    ProductId = productId,
                    Quantity = quantity
                });
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateQuantityAsync(string userId, int cartItemId, int quantity)
        {
            var cart = await GetCartByUserIdAsync(userId);
            var item = cart.CartItems.FirstOrDefault(ci => ci.Id == cartItemId);
            if (item != null)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null && quantity <= product.StockQuantity && quantity > 0)
                {
                    item.Quantity = quantity;
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task RemoveFromCartAsync(string userId, int cartItemId)
        {
            var cart = await GetCartByUserIdAsync(userId);
            var item = cart.CartItems.FirstOrDefault(ci => ci.Id == cartItemId);
            if (item != null)
            {
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
            }
        }

        public async Task ClearCartAsync(string userId)
        {
            var cart = await GetCartByUserIdAsync(userId);
            _context.CartItems.RemoveRange(cart.CartItems);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetCartItemsCountAsync(string userId)
        {
            var cart = await _context.ShoppingCarts
                .Include(sc => sc.CartItems)
                .FirstOrDefaultAsync(sc => sc.UserId == userId);

            return cart?.CartItems.Sum(ci => ci.Quantity) ?? 0;
        }
    }
}
