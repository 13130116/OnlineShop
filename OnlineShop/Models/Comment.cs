namespace OnlineShop.Models
{
    public class Comment
    {
        public int Id { get; set; }

        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // 關聯商品
        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}