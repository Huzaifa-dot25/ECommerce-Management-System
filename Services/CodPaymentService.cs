using E_com.Interfaces;
using E_com.Models;

namespace E_com.Services
{
    public class CodPaymentService : IPaymentService
    {
        public string PaymentMethodName => "Cash On Delivery";

        public Task<Payment> ProcessPaymentAsync(Order order)
        {
            // Cash on delivery payments are created in Pending state at order placement.
            var payment = new Payment
            {
                OrderId = order.Id,
                PaymentMethod = PaymentMethodName,
                PaymentStatus = "Pending",
                TransactionId = "COD-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                PaymentDate = DateTime.UtcNow
            };

            return Task.FromResult(payment);
        }
    }
}
