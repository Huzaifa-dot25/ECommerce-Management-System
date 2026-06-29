using E_com.Models;

namespace E_com.ViewModels
{
    public class HomeIndexViewModel
    {
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();
        public IEnumerable<Product> FeaturedProducts { get; set; } = new List<Product>();
    }
}
