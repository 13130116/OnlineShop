using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Data;
using OnlineShop.Models;
using Microsoft.AspNetCore.Authorization;

namespace OnlineShop.Controllers
{
    public class ProductsController : Controller
    {
        private readonly OnlineShopContext _context;

        public ProductsController(OnlineShopContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index(int? categoryId,string searchString,int? pageNumber)
        {
            var products = from p in _context.Product
                           select p;
            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value);
            }
            // 搜尋功能
            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.Name.Contains(searchString));
            }

            int pageSize = 5;

            if (!string.IsNullOrEmpty(searchString))
            {
                ViewData["Title"] = $"搜尋結果：{searchString}";
            }
            else
            {
                ViewData["Title"] = "商品列表";
            }

            // 分頁
            return View(await PaginatedList<Product>.CreateAsync(
                products.AsNoTracking(),
                pageNumber ?? 1,
                pageSize));
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product
            .Include(p => p.Comments.OrderByDescending(c => c.CreatedAt))
            .Include(p => p.ProductVariants)
            .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(Comment comment)
        {
            if (comment == null || string.IsNullOrWhiteSpace(comment.Content))
            {
                return RedirectToAction("Details", new { id = comment.ProductId });
            }

            comment.CreatedAt = DateTime.Now;
            comment.Content = comment.Content?.Trim();

            _context.Comment.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = comment.ProductId });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRestockNotification(int productVariantId, int productId)
        {
            var notification = new RestockNotification
            {
                ProductVariantId = productVariantId,
                UserId = "Guest",      // 之後如果有會員登入可以改成真正會員ID
                RequestDate = DateTime.Now,
                IsNotified = false
            };

            _context.RestockNotifications.Add(notification);
            await _context.SaveChangesAsync();

            TempData["Message"] = "已成功登記到貨通知！";

            return RedirectToAction("Details", new { id = productId });
        }


        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["CategoryId"] =
                new SelectList(_context.Category, "Id", "Name");
            return View();
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile == null || imageFile.Length == 0)
                {
                    ModelState.AddModelError("Image", "請上傳商品圖片");
                    ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Name", product.CategoryId);
                    return View(product);
                }

                using var ms = new MemoryStream();
                await imageFile.CopyToAsync(ms);
                product.Image = ms.ToArray();

                _context.Add(product);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Name", product.CategoryId);
            return View(product);
        }


        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Price,Stock")] Product product, IFormFile imageFile)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            // 移除導覽屬性或圖片檔案的驗證，避免因為 these 欄位沒填而報錯
            ModelState.Remove("imageFile");
            ModelState.Remove("Category");
            ModelState.Remove("Comments");

            if (ModelState.IsValid)
            {
                try
                {
                    var existingProduct = await _context.Product.FindAsync(id);
                    if (existingProduct == null)
                    {
                        return NotFound();
                    }

                    // 1. 更新主商品的基本欄位
                    existingProduct.Name = product.Name;
                    existingProduct.Price = product.Price;
                    existingProduct.Stock = product.Stock;

                    // 🌟 2. 自動同步更新底下的所有子規格庫存
                    var variants = await _context.ProductVariants
                        .Where(v => v.ProductId == id)
                        .ToListAsync();

                    foreach (var variant in variants)
                    {
                        variant.Stock = product.Stock; // 讓子規格庫存直接等於主商品庫存
                        _context.Update(variant);
                    }

                    // 3. 處理圖片上傳
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        using var ms = new MemoryStream();
                        await imageFile.CopyToAsync(ms);
                        existingProduct.Image = ms.ToArray();
                    }

                    _context.Update(existingProduct);
                    await _context.SaveChangesAsync(); // 一併儲存主商品與所有子規格
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }


        // GET: Products/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Product.FindAsync(id);
            if (product != null)
            {
                _context.Product.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public async Task<IActionResult> NotifyRestock(int variantId)
        {
            var variant = await _context.ProductVariants.FindAsync(variantId);

            if (variant == null)
            {
                return NotFound();
            }

            if (variant.Stock > 0)
            {
                var notifications = await _context.RestockNotifications
                    .Where(r => r.ProductVariantId == variantId && !r.IsNotified)
                    .ToListAsync();

                foreach (var item in notifications)
                {
                    item.IsNotified = true;
                }

                await _context.SaveChangesAsync();
            }

            return Ok();
        }

        private bool ProductExists(int id)
        {
            return _context.Product.Any(e => e.Id == id);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult CreateCategory()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(Category category)
        {
            _context.Add(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}