namespace OnlineShop.Models
{
    public class CartItem
    {
        public int ProductId { get; set; }

        // 新增：記錄是哪一個顏色+容量規格
        public int ProductVariantId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        // 新增：顯示規格
        public string Color { get; set; } = string.Empty;
        public string Capacity { get; set; } = string.Empty;

        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int Stock { get; set; }

        public decimal SubTotal => Price * Quantity;
    }

    public class Order
    {
        public string OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public List<CartItem> Items { get; set; }
        public decimal TotalAmount => Items.Sum(i => i.SubTotal);
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string PaymentMethod { get; set; }
        public string Email { get; set; }
    }
}