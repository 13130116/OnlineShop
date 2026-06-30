using System; 
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Data;       
using OnlineShop.Models;
using OnlineShop.Helpers;

namespace OnlineShop.Controllers
{
    public class CartController : Controller
    {
        // 1. 新增資料庫連線 (讓購物車能跟資料庫溝通扣庫存)
        private readonly OnlineShopContext _context;

        public CartController(OnlineShopContext context)
        {
            _context = context;
        }

        // 1. 顯示購物車結帳頁面
        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            return View(cart);
        }

        // 2. 加入購物車邏輯
        [HttpPost]
        public IActionResult AddToCart(int productId, string name, decimal price, int stock, int quantity = 1)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var existingItem = cart.FirstOrDefault(c => c.ProductId == productId);

            if (stock < 1)
            {
                TempData["ErrorMessage"] = $"抱歉，【{name}】已售完！";
                return RedirectToAction("Index", "Products");
            }

            if (existingItem != null)
            {
                if (existingItem.Quantity + quantity > stock)
                {
                    TempData["ErrorMessage"] = $"庫存不足！購物車內已有 {existingItem.Quantity} 個，最多只能再加 {stock - existingItem.Quantity} 個喔！";
                    return RedirectToAction("Index", "Products");
                }

                existingItem.Quantity += quantity;
            }
            else
            {
                if (quantity > stock)
                {
                    TempData["ErrorMessage"] = $"庫存不足！這項商品最多只能買 {stock} 個喔！";
                    return RedirectToAction("Index", "Products");
                }

                cart.Add(new CartItem { ProductId = productId, ProductName = name, Price = price, Quantity = quantity, Stock = stock });
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);
            TempData["SuccessMessage"] = $"太棒了！已成功將 {quantity} 份【{name}】加入購物車！🛒";

            return RedirectToAction("Index", "Products");
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

        // 6. 處理部分商品勾選結帳 
        [HttpPost]
        public async Task<IActionResult> Checkout(List<int> selectedProductIds)
        {
            if (selectedProductIds == null || !selectedProductIds.Any())
            {
                TempData["ErrorMessage"] = "請至少選擇一項商品進行結帳！";
                return RedirectToAction("Index");
            }

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var checkoutItems = cart.Where(c => selectedProductIds.Contains(c.ProductId)).ToList();

            // 去資料庫把真正商品的庫存扣掉！
            foreach (var item in checkoutItems)
            {
                var product = await _context.Product.FindAsync(item.ProductId);
                if (product != null)
                {
                    // 扣除庫存 (防呆：確保庫存不會扣到變成負數)
                    product.Stock = (product.Stock >= item.Quantity) ? (product.Stock - item.Quantity) : 0;
                    _context.Update(product);
                }
            }
            // 儲存進資料庫
            await _context.SaveChangesAsync();

            // 打包存進「歷史訂單 Session」
            var orderHistory = HttpContext.Session.GetObjectFromJson<List<Order>>("OrderHistory") ?? new List<Order>();
            var newOrder = new Order
            {
                // 使用目前時間做為不重複的訂單編號
                OrderId = "ORD" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                OrderDate = DateTime.Now,
                Items = checkoutItems
            };
            orderHistory.Add(newOrder);
            HttpContext.Session.SetObjectAsJson("OrderHistory", orderHistory); // 存回 Session

            // 把已經結帳的商品從購物車裡刪掉 (買完就該清空囉！)
            cart.RemoveAll(c => selectedProductIds.Contains(c.ProductId));
            HttpContext.Session.SetObjectAsJson("Cart", cart);

            return View("CheckoutSummary", checkoutItems);
        }

        // 新增：顯示歷史訂單紀錄頁面
        public IActionResult OrderHistory()
        {
            // 從 Session 撈出所有購買過的訂單，如果沒有就給空清單
            var orderHistory = HttpContext.Session.GetObjectFromJson<List<Order>>("OrderHistory") ?? new List<Order>();

            // 讓最新的訂單排在最上面顯示，並轉成 List 傳給 View
            var sortedOrders = orderHistory.OrderByDescending(o => o.OrderDate).ToList();

            return View(sortedOrders);
        }
    }
}