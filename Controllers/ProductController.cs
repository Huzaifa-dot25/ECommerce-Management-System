using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using E_com.Interfaces;
using E_com.Models;

namespace E_com.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(
            IProductService productService, 
            ICategoryService categoryService, 
            IWebHostEnvironment webHostEnvironment)
        {
            _productService = productService;
            _categoryService = categoryService;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetProductsWithCategoryAsync();
            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateCategoriesDropdownAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile[] imageFiles)
        {
            if (ModelState.IsValid)
            {
                var imagePaths = new List<string>();
                if (imageFiles != null && imageFiles.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "products");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    foreach (var imageFile in imageFiles)
                    {
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(fileStream);
                        }

                        imagePaths.Add("/uploads/products/" + uniqueFileName);
                    }
                }

                await _productService.CreateProductAsync(product, imagePaths);
                TempData["SuccessMessage"] = "Product created successfully!";
                return RedirectToAction(nameof(Index));
            }

            await PopulateCategoriesDropdownAsync();
            return View(product);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productService.GetProductWithImagesAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            await PopulateCategoriesDropdownAsync(product.CategoryId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile[] imageFiles)
        {
            if (id != product.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var imagePaths = new List<string>();
                    if (imageFiles != null && imageFiles.Length > 0)
                    {
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "products");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        foreach (var imageFile in imageFiles)
                        {
                            string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
                            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await imageFile.CopyToAsync(fileStream);
                            }

                            imagePaths.Add("/uploads/products/" + uniqueFileName);
                        }
                    }

                    await _productService.UpdateProductAsync(product, imagePaths);
                    TempData["SuccessMessage"] = "Product updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again later.");
                }
            }

            await PopulateCategoriesDropdownAsync(product.CategoryId);
            // Reload original images for display
            var reloadedProduct = await _productService.GetProductWithImagesAsync(id);
            if (reloadedProduct != null)
            {
                product.ProductImages = reloadedProduct.ProductImages;
            }
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productService.GetProductWithImagesAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            // Delete physically stored images
            foreach (var img in product.ProductImages)
            {
                string filePath = Path.Combine(_webHostEnvironment.WebRootPath, img.ImagePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            await _productService.DeleteProductAsync(id);
            TempData["SuccessMessage"] = "Product deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage(int imageId, int productId)
        {
            var product = await _productService.GetProductWithImagesAsync(productId);
            if (product != null)
            {
                var image = product.ProductImages.FirstOrDefault(i => i.Id == imageId);
                if (image != null)
                {
                    // Delete physical file
                    string filePath = Path.Combine(_webHostEnvironment.WebRootPath, image.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }

                    await _productService.DeleteProductImageAsync(imageId);
                    
                    // If deleted image was primary, mark another one as primary if available
                    if (image.IsPrimary && product.ProductImages.Any(i => i.Id != imageId))
                    {
                        var nextImg = product.ProductImages.First(i => i.Id != imageId);
                        nextImg.IsPrimary = true;
                        await _productService.UpdateProductAsync(product, new List<string>());
                    }
                    
                    TempData["SuccessMessage"] = "Image deleted successfully!";
                }
            }
            return RedirectToAction(nameof(Edit), new { id = productId });
        }

        private async Task PopulateCategoriesDropdownAsync(object? selectedCategory = null)
        {
            var categories = await _categoryService.GetActiveCategoriesAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", selectedCategory);
        }
    }
}
