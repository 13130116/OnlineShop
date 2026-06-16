using Microsoft.EntityFrameworkCore;
using OnlineShop.Models;

namespace OnlineShop.Data
{
    public class OnlineShopContext : DbContext
    {
        public OnlineShopContext(DbContextOptions<OnlineShopContext> options)
            : base(options)
        {
        }

        public DbSet<OnlineShop.Models.Product> Product { get; set; } = default!;
        // 加上下面這行，讓資料庫認識 Category
        public DbSet<OnlineShop.Models.Category> Category { get; set; } = default!;

        public DbSet<Comment> Comment { get; set; }
    }
}