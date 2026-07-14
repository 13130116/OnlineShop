using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Data;

namespace OnlineShop.ViewComponents
{
    public class CategoryNavViewComponent : ViewComponent
    {
        private readonly OnlineShopContext _context;

        public CategoryNavViewComponent(OnlineShopContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var categories = await _context.Category
                .OrderBy(c => c.SortOrder)
                .ToListAsync();

            return View(categories);
        }
    }
}