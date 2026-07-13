using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Models
{
    public class CheckoutViewModel
    {
        [Display(Name = "訂購人姓名")]
        [Required(ErrorMessage = "請填寫訂購人姓名！")]
        [StringLength(50, ErrorMessage = "姓名長度不能超過 50 個字。")]
        public string FullName { get; set; }

        [Display(Name = "聯絡電話")]
        [Required(ErrorMessage = "請填寫聯絡電話！")]
        // 台灣手機/市話基本格式防呆驗證
        [RegularExpression(@"^09\d{8}$|^0\d{1,2}-?\d{6,8}$", ErrorMessage = "請填寫正確的電話號碼格式（如：0912345678 或 02-25001234）。")]
        public string Phone { get; set; }

        [Display(Name = "寄送地址")]
        [Required(ErrorMessage = "請填寫完整寄送地址！")]
        [MinLength(10, ErrorMessage = "地址太短囉，請填寫完整的縣市、區域與路段（至少 10 個字）。")]
        public string Address { get; set; }

        [Display(Name = "付款方式")]
        [Required(ErrorMessage = "請選擇付款方式！")]
        public string PaymentMethod { get; set; }
    }
}