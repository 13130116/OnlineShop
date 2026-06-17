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
        public IActionResult AddToCart(int productId, string name, decimal price, int stock)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var existingItem = cart.FirstOrDefault(c => c.ProductId == productId);

            if (existingItem != null)
            {
                if (existingItem.Quantity + 1 > stock)
                {
                    TempData["ErrorMessage"] = $"庫存不足！這項商品最多只能買 {stock} 個喔！";
                    return RedirectToAction("Index", "Home");
                }

                existingItem.Quantity++;
            }
            else
            {
                if (stock < 1)
                {
                    TempData["ErrorMessage"] = "抱歉，此商品已售完！";
                    return RedirectToAction("Index", "Home");
                }

                cart.Add(new CartItem { ProductId = productId, ProductName = name, Price = price, Quantity = 1, Stock = stock });
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);
            TempData["SuccessMessage"] = "商品已成功加入購物車！";

            return RedirectToAction("Index", "Home");
        }

        // 3. 即時更新購物車數量
        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var item = cart.FirstOrDefault(c => c.ProductId == productId);

            if (item != null)
            {
                if (quantity > item.Stock)
                {
                    return Json(new { success = false, message = "選購數量超過庫存上限！" });
                }

                item.Quantity = quantity;
                HttpContext.Session.SetObjectAsJson("Cart", cart);

                return Json(new { success = true });
            }

            return Json(new { success = false, message = "購物車中找不到該商品" });
        }

        // 4. 移除單一商品 
        [HttpPost]
        public IActionResult RemoveItem(int productId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var item = cart.FirstOrDefault(c => c.ProductId == productId);

            if (item != null)
            {
                cart.Remove(item);
                HttpContext.Session.SetObjectAsJson("Cart", cart);

                return Json(new { success = true, isEmpty = !cart.Any() });
            }

            return Json(new { success = false, message = "找不到該商品" });
        }

        // 5. 清空購物車
        [HttpPost]
        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove("Cart");

            return RedirectToAction("Index");
        }

        // 處理部分商品勾選結帳
        [HttpPost]
        public IActionResult Checkout(List<int> selectedProductIds)
        {
            
            if (selectedProductIds == null || !selectedProductIds.Any())
            {
                TempData["ErrorMessage"] = "請至少選擇一項商品進行結帳！";
                return RedirectToAction("Index");
            }

            TempData["SuccessMessage"] = $"準備進入結帳！您共勾選了 {selectedProductIds.Count} 項商品。";

            return RedirectToAction("Index");
        }
    }
}