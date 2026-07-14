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

namespace OnlineShop.Controllers
{
    public class ProductsController : Controller
    {
        private readonly OnlineShopContext _context;

        public ProductsController(OnlineShopContext context)
        {
            _context = context;
        }

        // --- Index ---
        public async Task<IActionResult> Index(int? categoryId)
        {
            var products = _context.Product.AsQueryable();
            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId);
            }
            return View(await products.ToListAsync());
        }

        // --- Edit (GET) ---
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Product.FindAsync(id);
            if (product == null) return NotFound();

            ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // --- Edit (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Price,Stock,Description,CategoryId")] Product product, IFormFile? imageFile)
        {
            if (id != product.Id) return NotFound();

            ModelState.Remove("ProductVariants");
            ModelState.Remove("Comments");

            if (ModelState.IsValid)
            {
                try
                {
                    var productToUpdate = await _context.Product.FindAsync(id);
                    if (productToUpdate == null) return NotFound();

                    productToUpdate.Name = product.Name;
                    productToUpdate.Price = product.Price;
                    productToUpdate.Stock = product.Stock;
                    productToUpdate.Description = product.Description;
                    productToUpdate.CategoryId = product.CategoryId;

                    if (imageFile != null && imageFile.Length > 0)
                    {
                        using var ms = new MemoryStream();
                        await imageFile.CopyToAsync(ms);
                        productToUpdate.Image = ms.ToArray();
                    }

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index), new { categoryId = product.CategoryId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Product.Any(e => e.Id == id)) return NotFound();
                    else throw;
                }
            }
            ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // --- Create (GET) ---
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Name");
            return View();
        }

        // --- Create (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Price,Stock,Description,CategoryId")] Product product, IFormFile? imageFile)
        {
            ModelState.Remove("ProductVariants");
            ModelState.Remove("Comments");

            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    using var ms = new MemoryStream();
                    await imageFile.CopyToAsync(ms);
                    product.Image = ms.ToArray();
                }
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { categoryId = product.CategoryId });
            }
            ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // --- Delete ---
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var product = await _context.Product.FirstOrDefaultAsync(m => m.Id == id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Product.FindAsync(id);
            if (product != null)
            {
                int catId = product.CategoryId ?? 0;
                _context.Product.Remove(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { categoryId = catId });
            }
            return RedirectToAction(nameof(Index));
        }
    }
}