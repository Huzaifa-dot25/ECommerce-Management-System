using E_com.Data;
using E_com.Interfaces;
using E_com.Models;

namespace E_com.Repositories
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
