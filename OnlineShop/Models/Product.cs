using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineShop.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Display(Name = "商品名稱")]
        public string Name { get; set; }

        [Display(Name = "價格")]
        public int Price { get; set; }

        [Display(Name = "商品簡介")]
        public string Description { get; set; } // 商品簡介

        [Display(Name = "商品內容")]
        public string Content { get; set; } // 商品內容

        [Display(Name = "庫存數量")]
        public int Stock { get; set; } // 商品庫存

        // 外鍵
        [Display(Name = "類別")]
        public int CategoryId { get; set; }

        [Display(Name = "商品圖片")]
        public byte[] Image { get; set; }

        // 導覽屬性
        public Category Category { get; set; }
        public List<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();
    }
}