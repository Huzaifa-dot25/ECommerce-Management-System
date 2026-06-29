using E_com.Models;

namespace E_com.Interfaces
{
    public interface IOrderService
    {
        Task<Order> CreateOrderAsync(string userId, Address address, string paymentMethod, string? couponCode = null);
        Task<Order?> GetOrderByIdAsync(int orderId);
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId);
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task UpdateOrderStatusAsync(int orderId, string orderStatus, string paymentStatus);
        Task CancelOrderAsync(int orderId);
    }
}
