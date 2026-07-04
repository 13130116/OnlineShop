using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineShop.Models
{
    public class RestockNotification
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "缺貨規格編號")]
        public int ProductVariantId { get; set; }

        [ForeignKey("ProductVariantId")]
        public virtual ProductVariant? ProductVariant { get; set; }

        [Display(Name = "會員帳號")]
        public string UserId { get; set; } = string.Empty;

        [Display(Name = "登記時間")]
        public DateTime RequestDate { get; set; } = DateTime.Now;

        [Display(Name = "是否已通知")]
        public bool IsNotified { get; set; } = false;
    }
}