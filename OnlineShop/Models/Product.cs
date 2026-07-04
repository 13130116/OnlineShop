using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineShop.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "商品名稱")]
        [Required(ErrorMessage = "請輸入商品名稱")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "商品描述")]
        public string Description { get; set; } = string.Empty;

        // 🟢 修正：完全保留 4 號原創的 byte[] 二進位圖片型態，絕不推翻組員成果！
        [Display(Name = "商品圖片")]
        public byte[] Image { get; set; } = Array.Empty<byte>();

        [Display(Name = "商品價格")]
        public decimal Price { get; set; }

        [Display(Name = "庫存數量")]
        public int Stock { get; set; }

        [Display(Name = "類別")]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        [Display(Name = "所屬類別")]
        public virtual Category? Category { get; set; }

        // 這次新增的子規格關聯
        public virtual ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();

        // 5 號原本的留言板關聯
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}