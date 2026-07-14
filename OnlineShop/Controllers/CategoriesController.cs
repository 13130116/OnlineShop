#nullable enable
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Data;
using OnlineShop.Models;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace OnlineShop.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly OnlineShopContext _context;
        public CategoriesController(OnlineShopContext context) => _context = context;

        // 1. 類別列表頁面
        public async Task<IActionResult> Index()
        {
            return View(await _context.Category.OrderBy(c => c.SortOrder).ToListAsync());
        }

        // 2. 新增類別 - 顯示頁面 (GET)
        public IActionResult Create()
        {
            return View();
        }

        // 3. 新增類別 - 處理資料 (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,SortOrder")] Category category, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await imageFile.CopyToAsync(memoryStream);
                        category.Image = memoryStream.ToArray();
                    }
                }

                var maxOrder = await _context.Category.MaxAsync(c => (int?)c.SortOrder) ?? 0;
                category.SortOrder = maxOrder + 1;

                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // 4. 編輯類別 - 顯示頁面 (GET)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Category.FindAsync(id);
            if (category == null) return NotFound();

            return View(category);
        }

        // 5. 編輯類別 - 處理資料 (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,SortOrder")] Category category, IFormFile? imageFile)
        {
            if (id != category.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await imageFile.CopyToAsync(memoryStream);
                            category.Image = memoryStream.ToArray();
                        }
                    }
                    else
                    {
                        // 若無上傳新圖，則保留原圖
                        var existing = await _context.Category.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
                        if (existing != null)
                        {
                            category.Image = existing.Image;
                        }
                    }

                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // 6. 刪除類別 - 顯示確認頁面 (GET)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Category.FirstOrDefaultAsync(m => m.Id == id);
            if (category == null) return NotFound();

            return View(category);
        }

        // 7. 刪除類別 - 確定刪除 (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Category.FindAsync(id);
            if (category != null)
            {
                _context.Category.Remove(category);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // 8. 排序：上移
        public async Task<IActionResult> MoveUp(int id)
        {
            var category = await _context.Category.FindAsync(id);
            if (category == null) return NotFound();

            var prev = await _context.Category.Where(c => c.SortOrder < category.SortOrder).OrderByDescending(c => c.SortOrder).FirstOrDefaultAsync();
            if (prev != null)
            {
                int temp = category.SortOrder;
                category.SortOrder = prev.SortOrder;
                prev.SortOrder = temp;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // 9. 排序：下移
        public async Task<IActionResult> MoveDown(int id)
        {
            var category = await _context.Category.FindAsync(id);
            if (category == null) return NotFound();

            var next = await _context.Category.Where(c => c.SortOrder > category.SortOrder).OrderBy(c => c.SortOrder).FirstOrDefaultAsync();
            if (next != null)
            {
                int temp = category.SortOrder;
                category.SortOrder = next.SortOrder;
                next.SortOrder = temp;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // 10. 排序：拖曳更新
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrder([FromBody] int[] ids)
        {
            for (int i = 0; i < ids.Length; i++)
            {
                var category = await _context.Category.FindAsync(ids[i]);
                if (category != null)
                {
                    category.SortOrder = i;
                }
            }
            await _context.SaveChangesAsync();
            return Ok();
        }

        private bool CategoryExists(int id)
        {
            return _context.Category.Any(e => e.Id == id);
        }
    }
}