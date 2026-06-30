using E_com.Interfaces;
using E_com.Models;

namespace E_com.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _productRepository.GetAllAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsWithCategoryAsync()
        {
            return await _productRepository.GetProductsWithCategoryAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _productRepository.GetByIdAsync(id);
        }

        public async Task<Product?> GetProductWithImagesAsync(int id)
        {
            return await _productRepository.GetProductWithImagesAsync(id);
        }

        public async Task CreateProductAsync(Product product, List<string> imagePaths)
        {
            product.CreatedDate = DateTime.UtcNow;
            
            // Add images
            for (int i = 0; i < imagePaths.Count; i++)
            {
                product.ProductImages.Add(new ProductImage
                {
                    ImagePath = imagePaths[i],
                    IsPrimary = i == 0 // First image is primary
                });
            }

            await _productRepository.AddAsync(product);
            await _productRepository.SaveChangesAsync();
        }

        public async Task UpdateProductAsync(Product product, List<string> newImagePaths)
        {
            var existingProduct = await _productRepository.GetProductWithImagesAsync(product.Id);
            if (existingProduct == null) return;

            // Update main properties
            existingProduct.Name = product.Name;
            existingProduct.SKU = product.SKU;
            existingProduct.CategoryId = product.CategoryId;
            existingProduct.Brand = product.Brand;
            existingProduct.Description = product.Description;
            existingProduct.Price = product.Price;
            existingProduct.Discount = product.Discount;
            existingProduct.StockQuantity = product.StockQuantity;
            existingProduct.Status = product.Status;

            // Determine if there are already any primary images
            bool hasPrimary = existingProduct.ProductImages.Any(pi => pi.IsPrimary);

            // Add new images
            for (int i = 0; i < newImagePaths.Count; i++)
            {
                existingProduct.ProductImages.Add(new ProductImage
                {
                    ImagePath = newImagePaths[i],
                    IsPrimary = !hasPrimary && (i == 0) // Mark primary if none exists
                });
            }

            _productRepository.Update(existingProduct);
            await _productRepository.SaveChangesAsync();
        }

        public async Task DeleteProductAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product != null)
            {
                _productRepository.Remove(product);
                await _productRepository.SaveChangesAsync();
            }
        }

        public async Task DeleteProductImageAsync(int imageId)
        {
            await _productRepository.DeleteProductImageAsync(imageId);
        }

        public async Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold = 10)
        {
            return await _productRepository.GetLowStockProductsAsync(threshold);
        }

        public async Task<IEnumerable<Product>> GetFeaturedProductsAsync(int count)
        {
            var products = await _productRepository.GetProductsWithCategoryAsync();
            return products.Where(p => p.Status).Take(count);
        }

        public async Task<(IEnumerable<Product> Products, int TotalCount)> SearchProductsAsync(
            string? search, 
            int? categoryId, 
            decimal? minPrice, 
            decimal? maxPrice, 
            string? sortBy, 
            int page, 
            int pageSize)
        {
            var allProducts = await _productRepository.GetProductsWithCategoryAsync();
            var query = allProducts.Where(p => p.Status);

            // Filters
            if (!string.IsNullOrWhiteSpace(search))
            {
                var lowerSearch = search.Trim().ToLowerInvariant();
                query = query.Where(p =>
                    p.Name.ToLowerInvariant().Contains(lowerSearch) ||
                    p.Brand.ToLowerInvariant().Contains(lowerSearch) ||
                    p.SKU.ToLowerInvariant().Contains(lowerSearch) ||
                    p.Description.ToLowerInvariant().Contains(lowerSearch));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.DiscountedPrice >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.DiscountedPrice <= maxPrice.Value);
            }

            // Sorting
            query = sortBy switch
            {
                "price_asc" => query.OrderBy(p => p.DiscountedPrice),
                "price_desc" => query.OrderByDescending(p => p.DiscountedPrice),
                "newest" => query.OrderByDescending(p => p.CreatedDate),
                "discount" => query.OrderByDescending(p => p.Discount),
                _ => query.OrderByDescending(p => p.CreatedDate) // Default
            };

            int totalCount = query.Count();

            // Pagination
            var pagedProducts = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (pagedProducts, totalCount);
        }
    }
}
