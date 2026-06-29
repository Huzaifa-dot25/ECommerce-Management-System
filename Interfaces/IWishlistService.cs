using E_com.Models;

namespace E_com.Interfaces
{
    public interface IWishlistService
    {
        Task<IEnumerable<Wishlist>> GetWishlistByUserIdAsync(string userId);
        Task AddToWishlistAsync(string userId, int productId);
        Task RemoveFromWishlistAsync(string userId, int wishlistId);
        Task<bool> IsInWishlistAsync(string userId, int productId);
        Task MoveToCartAsync(string userId, int wishlistId);
    }
}
