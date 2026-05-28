namespace OnlineShop.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        
        // 一個類別有很多商品
        public List<Product>? Products { get; set; }
    }
}