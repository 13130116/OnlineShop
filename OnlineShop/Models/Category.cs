#nullable enable
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace OnlineShop.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "請輸入類別名稱")]
        public string Name { get; set; } = string.Empty;

        public byte[]? Image { get; set; }

        // 改回 SortOrder，與你的資料庫 Migration 完全一致
        public int SortOrder { get; set; } = 0;

        public virtual ICollection<Product>? Products { get; set; }
    }
}