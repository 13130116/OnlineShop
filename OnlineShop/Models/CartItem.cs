namespace OnlineShop.Models
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal SubTotal => Price * Quantity;

        // 🌟 只需要新增下面這一行，讓購物車能記住該商品的庫存上限
        public int Stock { get; set; }
    }
}