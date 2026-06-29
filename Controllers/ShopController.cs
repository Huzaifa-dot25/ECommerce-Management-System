using Microsoft.AspNetCore.Mvc;
using E_com.Interfaces;
using E_com.ViewModels;

namespace E_com.Controllers
{
    public class ShopController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public ShopController(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index(
            string? search, 
            int? categoryId, 
            decimal? minPrice, 
            decimal? maxPrice, 
            string? sortBy, 
            int page = 1, 
            int pageSize = 9)
        {
            var (products, totalCount) = await _productService.SearchProductsAsync(
                search, categoryId, minPrice, maxPrice, sortBy, page, pageSize);

            var categories = await _categoryService.GetActiveCategoriesAsync();

            var viewModel = new ShopIndexViewModel
            {
                Products = products,
                Categories = categories,
                Search = search,
                CategoryId = categoryId,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                SortBy = sortBy,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetProductWithImagesAsync(id);
            if (product == null || !product.Status)
            {
                return NotFound();
            }

            // We will fetch reviews (only approved ones) and pass to ViewBag/model
            var approvedReviews = product.Reviews.Where(r => r.Status).OrderByDescending(r => r.ReviewDate).ToList();
            ViewBag.Reviews = approvedReviews;
            
            // Calculate average rating
            ViewBag.AverageRating = approvedReviews.Any() ? approvedReviews.Average(r => r.Rating) : 0.0;

            // Load related products from the same category
            var allProducts = await _productService.GetProductsWithCategoryAsync();
            var relatedProducts = allProducts
                .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id && p.Status)
                .Take(4)
                .ToList();
            ViewBag.RelatedProducts = relatedProducts;

            return View(product);
        }
    }
}
