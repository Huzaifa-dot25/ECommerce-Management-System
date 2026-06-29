using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using E_com.Models;
using E_com.Interfaces;
using E_com.ViewModels;

namespace E_com.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            ICategoryService categoryService, 
            IProductService productService, 
            ILogger<HomeController> logger)
        {
            _categoryService = categoryService;
            _productService = productService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetActiveCategoriesAsync();
            var featuredProducts = await _productService.GetFeaturedProductsAsync(8);

            var viewModel = new HomeIndexViewModel
            {
                Categories = categories,
                FeaturedProducts = featuredProducts
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
