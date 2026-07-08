using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Models;

namespace OnlineShop.Data
{
    public class OnlineShopContext : IdentityDbContext<ApplicationUser>
    {
        public OnlineShopContext(DbContextOptions<OnlineShopContext> options)
            : base(options)
        {
        }

        // 修正：全部改回符合舊程式碼呼叫的單數型態名稱
        public DbSet<Category> Category { get; set; } = null!;
        public DbSet<Product> Product { get; set; } = null!;

        // 保留：5 號原本就建立好的舊留言板
        public DbSet<Comment> Comment { get; set; } = null!;

        // 新增：這次開會決定加入的全新功能資料表
        public DbSet<ProductVariant> ProductVariants { get; set; } = null!;
        public DbSet<Consultation> Consultations { get; set; } = null!;
        public DbSet<RestockNotification> RestockNotifications { get; set; } = null!;
    }
}