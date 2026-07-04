using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using OnlineShop.Models;

namespace OnlineShop.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "類別名稱")]
        [Required(ErrorMessage = "請輸入類別名稱")]
        public string Name { get; set; } = string.Empty;

        // 關聯：一個類別會有多個商品
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}