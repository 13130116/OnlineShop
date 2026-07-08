using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Data;       
using OnlineShop.Helpers;
using OnlineShop.Models;
using System; 
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


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
        public async Task<IActionResult> AddToCart(int productId, int variantId, int quantity = 1)
        {
            var variant = await _context.ProductVariants
                .Include(v => v.Product)
                .FirstOrDefaultAsync(v => v.Id == variantId);

            if (variant == null)
            {
                TempData["ErrorMessage"] = "找不到商品規格！";
                return RedirectToAction("Details", "Products", new { id = productId });
            }

            if (variant.Stock < quantity)
            {
                TempData["ErrorMessage"] = $"抱歉，【{variant.Product?.Name} {variant.Color} {variant.Capacity}】庫存不足！";
                return RedirectToAction("Details", "Products", new { id = productId });
            }

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

            var existingItem = cart.FirstOrDefault(c => c.ProductVariantId == variantId);

            if (existingItem != null)
            {
                if (existingItem.Quantity + quantity > variant.Stock)
                {
                    TempData["ErrorMessage"] = "購物車數量已超過此規格庫存！";
                    return RedirectToAction("Details", "Products", new { id = productId });
                }

                existingItem.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId = productId,
                    ProductVariantId = variantId,
                    ProductName = $"{variant.Product?.Name} - {variant.Color} / {variant.Capacity}",
                    Price = variant.Price,
                    Quantity = quantity,
                    Stock = variant.Stock,
                    Color = variant.Color,
                    Capacity = variant.Capacity
                });
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);

            TempData["SuccessMessage"] = "已加入購物車！";

            return RedirectToAction("Details", "Products", new { id = productId });
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
            // 去資料庫扣 ProductVariant 的庫存
            foreach (var item in checkoutItems)
            {
                var variant = await _context.ProductVariants.FindAsync(item.ProductVariantId);

                if (variant != null)
                {
                    if (variant.Stock >= item.Quantity)
                    {
                        variant.Stock -= item.Quantity;
                    }
                    else
                    {
                        variant.Stock = 0;
                    }

                    _context.Update(variant);
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