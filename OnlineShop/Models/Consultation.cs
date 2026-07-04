using System;
using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Models
{
    public class Consultation
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "會員帳號")]
        public string UserId { get; set; } = string.Empty;

        [Display(Name = "許願/提問內容")]
        [Required(ErrorMessage = "請填寫您的問題")]
        public string Question { get; set; } = string.Empty;

        [Display(Name = "管理員回覆")]
        public string Answer { get; set; } = string.Empty;

        [Display(Name = "發問時間")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "是否已回覆")]
        public bool IsReplied { get; set; } = false;
    }
}