using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Display(Name = "類別名稱")]
        public string Name { get; set; }

        // 一個類別有很多商品
        public List<Product> Products { get; set; }
    }
}