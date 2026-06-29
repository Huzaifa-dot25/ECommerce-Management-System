using E_com.Models;

namespace E_com.Interfaces
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<IEnumerable<Product>> GetProductsWithCategoryAsync();
        Task<Product?> GetProductWithImagesAsync(int id);
        Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold);
        Task DeleteProductImageAsync(int imageId);
    }
}
