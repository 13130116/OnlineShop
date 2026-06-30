namespace OnlineShop.Models
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
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
    }
}