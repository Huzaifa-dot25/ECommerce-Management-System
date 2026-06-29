using E_com.Models;

namespace E_com.Interfaces
{
    public interface IPaymentService
    {
        string PaymentMethodName { get; }
        Task<Payment> ProcessPaymentAsync(Order order);
    }
}
