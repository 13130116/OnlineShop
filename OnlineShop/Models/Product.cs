#nullable enable
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace OnlineShop.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "您尚未填入商品名稱")]
        [Display(Name = "商品名稱")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "您尚未填入商品價格")]
        [Display(Name = "商品價格")]
        [Range(0, 99999999, ErrorMessage = "價格必須大於 0")]
        public int? Price { get; set; }

        // 👇 就是少了這個！補回被誤刪的庫存欄位，解決資料庫當機問題
        [Required(ErrorMessage = "您尚未填入商品庫存")]
        [Display(Name = "商品庫存")]
        [Range(0, 999999, ErrorMessage = "庫存不能為負數")]
        public int? Stock { get; set; }

        [Required(ErrorMessage = "您尚未填入類別")]
        [Display(Name = "類別")]
        public int? CategoryId { get; set; }

        public byte[]? Image { get; set; }

        public string? Description { get; set; }

        public virtual ICollection<ProductVariant>? ProductVariants { get; set; }
        public virtual ICollection<Comment>? Comments { get; set; }
    }
}