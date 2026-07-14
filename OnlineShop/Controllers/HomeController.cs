using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Data;

namespace OnlineShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly OnlineShopContext _context;
        public HomeController(OnlineShopContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            // 這裡必須強制使用 OrderBy(c => c.SortOrder)
            var categories = await _context.Category.OrderBy(c => c.SortOrder).ToListAsync();
            return View(categories);
        }
    }
}