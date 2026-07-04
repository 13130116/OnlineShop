using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineShop.Models
{
    public class ProductVariant
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "主商品")]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        [Display(Name = "顏色")]
        [Required(ErrorMessage = "請輸入顏色")]
        public string Color { get; set; } = string.Empty;

        [Display(Name = "容量")]
        [Required(ErrorMessage = "請輸入容量")]
        public string Capacity { get; set; } = string.Empty;

        [Display(Name = "價格")]
        [Required(ErrorMessage = "請輸入價格")]
        public decimal Price { get; set; }

        [Display(Name = "庫存數量")]
        [Required(ErrorMessage = "請輸入庫存數量")]
        public int Stock { get; set; }
    }
}