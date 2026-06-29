using Microsoft.EntityFrameworkCore;
using E_com.Data;
using E_com.Interfaces;
using E_com.Models;

namespace E_com.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICartService _cartService;
        private readonly IEnumerable<IPaymentService> _paymentServices;

        public OrderService(
            ApplicationDbContext context, 
            ICartService cartService, 
            IEnumerable<IPaymentService> paymentServices)
        {
            _context = context;
            _cartService = cartService;
            _paymentServices = paymentServices;
        }

        public async Task<Order> CreateOrderAsync(string userId, Address address, string paymentMethod, string? couponCode = null)
        {
            var cart = await _cartService.GetCartByUserIdAsync(userId);
            if (cart == null || !cart.CartItems.Any())
            {
                throw new InvalidOperationException("Cannot place order with an empty cart.");
            }

            // Verify stock and calculate subtotal
            decimal subTotal = 0.00m;
            var orderItems = new List<OrderItem>();

            foreach (var item in cart.CartItems)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null)
                {
                    throw new KeyNotFoundException($"Product with ID {item.ProductId} not found.");
                }

                if (product.StockQuantity < item.Quantity)
                {
                    throw new InvalidOperationException($"Insufficient stock for product: {product.Name}");
                }

                // Deduct stock
                product.StockQuantity -= item.Quantity;
                _context.Products.Update(product);

                // Add to order items list
                decimal unitPrice = product.Price;
                decimal itemDiscount = product.Discount;
                subTotal += (product.DiscountedPrice * item.Quantity);

                orderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = unitPrice,
                    Discount = itemDiscount
                });
            }

            // Check and apply coupon
            decimal discountAmount = 0.00m;
            if (!string.IsNullOrEmpty(couponCode))
            {
                var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.CouponCode == couponCode && c.Status && c.ExpiryDate >= DateTime.UtcNow);
                if (coupon != null)
                {
                    discountAmount = subTotal * (coupon.DiscountPercentage / 100);
                }
            }

            decimal totalAmount = subTotal - discountAmount;
            if (totalAmount < 0) totalAmount = 0.00m;

            // Create Order
            var order = new Order
            {
                OrderNumber = "ORD-" + DateTime.UtcNow.ToString("yyyyMMdd") + "-" + new Random().Next(1000, 9999),
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                SubTotal = subTotal,
                DiscountAmount = discountAmount,
                TotalAmount = totalAmount,
                ShippingAddress = address.StreetAddress,
                City = address.City,
                PostalCode = address.PostalCode,
                Phone = address.Phone,
                OrderStatus = "Pending",
                PaymentStatus = "Pending",
                CouponCode = couponCode,
                OrderItems = orderItems
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync(); // Generates order ID

            // Process Payment
            var paymentService = _paymentServices.FirstOrDefault(p => p.PaymentMethodName == paymentMethod);
            if (paymentService == null)
            {
                throw new NotSupportedException($"Payment method: '{paymentMethod}' is not supported.");
            }

            var payment = await paymentService.ProcessPaymentAsync(order);
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // Clear shopping cart
            await _cartService.ClearCartAsync(userId);

            return order;
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ThenInclude(p => p!.ProductImages)
                .Include(o => o.Payment)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Payment)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Payment)
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task UpdateOrderStatusAsync(int orderId, string orderStatus, string paymentStatus)
        {
            var order = await _context.Orders.Include(o => o.Payment).FirstOrDefaultAsync(o => o.Id == orderId);
            if (order != null)
            {
                order.OrderStatus = orderStatus;
                order.PaymentStatus = paymentStatus;
                
                if (order.Payment != null)
                {
                    order.Payment.PaymentStatus = paymentStatus;
                    _context.Payments.Update(order.Payment);
                }

                _context.Orders.Update(order);
                await _context.SaveChangesAsync();
            }
        }

        public async Task CancelOrderAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order != null && order.OrderStatus == "Pending")
            {
                order.OrderStatus = "Cancelled";
                order.PaymentStatus = "Cancelled";

                // Restore stock
                foreach (var item in order.OrderItems)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity += item.Quantity;
                        _context.Products.Update(product);
                    }
                }

                _context.Orders.Update(order);
                await _context.SaveChangesAsync();
            }
        }
    }
}
