using E_com.Models;

namespace E_com.Interfaces
{
    public interface ICartService
    {
        Task<ShoppingCart> GetCartByUserIdAsync(string userId);
        Task AddToCartAsync(string userId, int productId, int quantity);
        Task UpdateQuantityAsync(string userId, int cartItemId, int quantity);
        Task RemoveFromCartAsync(string userId, int cartItemId);
        Task ClearCartAsync(string userId);
        Task<int> GetCartItemsCountAsync(string userId);
    }
}
