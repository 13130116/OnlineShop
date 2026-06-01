using Microsoft.AspNetCore.Mvc;
using OnlineShop.Models;
using OnlineShop.Helpers;

namespace OnlineShop.Controllers
{
    public class CartController : Controller
    {
        // 1. 顯示購物車結帳頁面
        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            return View(cart);
        }

        // 2. 加入購物車邏輯
        public IActionResult AddToCart(int productId, string name, decimal price)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var existingItem = cart.FirstOrDefault(c => c.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                cart.Add(new CartItem { ProductId = productId, ProductName = name, Price = price, Quantity = 1 });
            }

            // 存回 Session 並設定成功動畫文字
            HttpContext.Session.SetObjectAsJson("Cart", cart);
            TempData["SuccessMessage"] = "商品已成功加入購物車！";

            // 回到原本的商品列表頁 (假設 Controller 叫 Home，可依隊友設定調整)
            return RedirectToAction("Index", "Home");
        }
    }
}