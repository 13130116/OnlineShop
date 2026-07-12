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

        // 2. 加入購物車邏輯 (完美保留你的 ProductVariant 邏輯)
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


        // 接收購物車頁面勾選的商品，並準備跳轉
        [HttpPost]
        public IActionResult Checkout(List<int> selectedProductIds)
        {
            if (selectedProductIds == null || !selectedProductIds.Any())
            {
                TempData["ErrorMessage"] = "請至少選擇一項商品進行結帳！";
                return RedirectToAction("Index");
            }

            // 把勾選的商品 ID 暫存在 Session，準備帶去下一頁
            HttpContext.Session.SetObjectAsJson("SelectedForCheckout", selectedProductIds);

            return RedirectToAction("CheckoutInfo");
        }

        // 步驟 6-2: 檢查登入狀態，並顯示「資料填寫表單」
        [HttpGet]
        public IActionResult CheckoutInfo()
        {
            // 確認有沒有帶過來的結帳商品
            var selectedIds = HttpContext.Session.GetObjectFromJson<List<int>>("SelectedForCheckout");
            if (selectedIds == null || !selectedIds.Any()) return RedirectToAction("Index");

            // 關鍵任務：訪客寬鬆模式 (未登入強制導向 Login，並帶上 ReturnUrl)
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                TempData["ErrorMessage"] = "結帳前請先登入或註冊會員喔！";
                // 這裡會跳轉到負責人 5 的登入頁面，登入後會自動導回來這頁
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("CheckoutInfo", "Cart") });
            }

            // 若已登入，抓出購物車內被選中的商品，傳給前端顯示明細
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var checkoutItems = cart.Where(c => selectedIds.Contains(c.ProductId)).ToList();

            ViewBag.CartItems = checkoutItems;
            return View(new CheckoutViewModel()); // 準備空的表單給使用者填寫
        }

        // 接收使用者填寫的表單，正式扣庫存並送出訂單
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitOrder(CheckoutViewModel model)
        {
            var selectedIds = HttpContext.Session.GetObjectFromJson<List<int>>("SelectedForCheckout");
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var checkoutItems = cart.Where(c => selectedIds != null && selectedIds.Contains(c.ProductId)).ToList();

            // 防呆：如果表單沒填好，退回原頁面並顯示紅字錯誤
            if (!ModelState.IsValid || !checkoutItems.Any())
            {
                ViewBag.CartItems = checkoutItems;
                return View("CheckoutInfo", model);
            }

            // 去資料庫把真正商品的庫存扣掉！(保留你的 ProductVariant 邏輯)
            foreach (var item in checkoutItems)
            {
                var variant = await _context.ProductVariants.FindAsync(item.ProductVariantId);
                if (variant != null)
                {
                    variant.Stock = (variant.Stock >= item.Quantity) ? (variant.Stock - item.Quantity) : 0;
                    _context.Update(variant);
                }
            }
            await _context.SaveChangesAsync();

            // 將表單資訊一起打包存進「歷史訂單 Session」
            var orderHistory = HttpContext.Session.GetObjectFromJson<List<Order>>("OrderHistory") ?? new List<Order>();
            var newOrder = new Order
            {
                OrderId = "ORD" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                OrderDate = DateTime.Now,
                Items = checkoutItems,
                // 把 model 裡的資料存進訂單裡
                FullName = model.FullName,
                Phone = model.Phone,
                Address = model.Address,
                PaymentMethod = model.PaymentMethod,
                // 🌟 新增：自動抓取目前登入使用者的帳號名稱 (Email)
                Email = User.Identity?.Name
            };
            orderHistory.Add(newOrder);
            HttpContext.Session.SetObjectAsJson("OrderHistory", orderHistory);

            // 把已經結帳的商品從購物車與暫存裡刪掉
            cart.RemoveAll(c => selectedIds.Contains(c.ProductId));
            HttpContext.Session.SetObjectAsJson("Cart", cart);
            HttpContext.Session.Remove("SelectedForCheckout");

            TempData["SuccessMessage"] = $"感謝 {model.FullName} 的訂購！訂單已正式成立。";

            // 把整張「新訂單」(newOrder) 傳給結帳完成頁面，讓畫面可以印出姓名地址
            return View("CheckoutSummary", newOrder);
        }



        // 7. 顯示歷史訂單紀錄頁面
        public IActionResult OrderHistory()
        {
            var orderHistory = HttpContext.Session.GetObjectFromJson<List<Order>>("OrderHistory") ?? new List<Order>();
            var sortedOrders = orderHistory.OrderByDescending(o => o.OrderDate).ToList();
            return View(sortedOrders);
        }
    }
}