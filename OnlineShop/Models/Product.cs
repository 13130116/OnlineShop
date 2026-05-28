using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineShop.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
        public string Description { get; set; } // 商品簡介

        public string Content { get; set; } // 商品內容
        public int Stock { get; set; } // 商品庫存
        // 外鍵
        public int CategoryId { get; set; }
        public byte[] Image { get; set; }
        // 導覽屬性
        public Category Category { get; set; }
    }
}