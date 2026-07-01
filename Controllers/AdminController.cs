using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using E_com.Interfaces;

namespace E_com.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public AdminController(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Dashboard()
        {
            var products   = await _productService.GetProductsWithCategoryAsync();
            var categories = await _categoryService.GetAllCategoriesAsync();
            var lowStock   = await _productService.GetLowStockProductsAsync(10);

            ViewBag.TotalProducts   = products.Count();
            ViewBag.ActiveProducts  = products.Count(p => p.Status);
            ViewBag.TotalCategories = categories.Count();
            ViewBag.LowStockCount   = lowStock.Count();
            ViewBag.LowStockItems   = lowStock.Take(5).ToList();
            ViewBag.RecentProducts  = products.Take(6).ToList();

            return View();
        }
    }
}
