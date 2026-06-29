using E_com.Models;

namespace E_com.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<IEnumerable<Product>> GetProductsWithCategoryAsync();
        Task<Product?> GetProductByIdAsync(int id);
        Task<Product?> GetProductWithImagesAsync(int id);
        Task CreateProductAsync(Product product, List<string> imagePaths);
        Task UpdateProductAsync(Product product, List<string> newImagePaths);
        Task DeleteProductAsync(int id);
        Task DeleteProductImageAsync(int imageId);
        Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold = 10);
        Task<IEnumerable<Product>> GetFeaturedProductsAsync(int count);
        
        // Advanced Store/Search capabilities
        Task<(IEnumerable<Product> Products, int TotalCount)> SearchProductsAsync(
            string? search, 
            int? categoryId, 
            decimal? minPrice, 
            decimal? maxPrice, 
            string? sortBy, 
            int page, 
            int pageSize);
    }
}
