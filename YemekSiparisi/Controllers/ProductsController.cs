// Menüye yemek ekleme, güncelleme, silme ve arama işlemleri burada yapılır

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using YemekSiparisi.Data;
using YemekSiparisi.Models;

namespace YemekSiparisi.Controllers
{
    [Tags("3. Menü ve Yemek Yönetimi")]

    [Route("api/Yemekler")]

    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // Belirli bir restorana ait ürünleri getirir
        [HttpGet("restaurant/{restaurantId}")]
        public async Task<IActionResult> GetProductsByRestaurant(int restaurantId)
        {
            var products = await _context.Products
                .Where(p => p.RestaurantId == restaurantId && !p.IsDeleted)
                .ToListAsync();

            return Ok(products);
        }

        // Yeni ürün ekleme işlemi
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateProduct(Product product)
        {
            // Token içinden kullanıcı id alınır
            var userId = int.Parse(User.FindFirstValue("userId")!);

            // Ürünün ait olduğu restoran bulunur
            var restaurant = await _context.Restaurants.FindAsync(product.RestaurantId);

            if (restaurant == null)
                return NotFound("Restoran bulunamadı.");

            // Sadece restoran sahibi ürün ekleyebilir
            if (restaurant.OwnerId != userId)
                return Forbid("Sadece kendi restoranınıza ürün ekleyebilirsiniz.");

            // Döngüsel referansı engellemek için navigation temizlenir
            product.Restaurant = null;

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Yemek menüye eklendi!", product });
        }

        // Ürün güncelleme işlemi
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, Product updatedProduct)
        {
            var product = await _context.Products
                .Include(p => p.Restaurant)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound("Ürün bulunamadı.");

            var userId = int.Parse(User.FindFirstValue("userId")!);

            // Sadece ürünün sahibi olan restoran güncelleyebilir
            if (product.Restaurant!.OwnerId != userId)
                return Forbid();

            // Alanlar güncellenir
            product.Name = updatedProduct.Name;
            product.Description = updatedProduct.Description;
            product.Price = updatedProduct.Price;
            product.Stock = updatedProduct.Stock;
            product.ImageUrl = updatedProduct.ImageUrl;

            await _context.SaveChangesAsync();

            return Ok("Ürün başarıyla güncellendi.");
        }

        // Ürün silme işlemi
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.Restaurant)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null || product.IsDeleted)
                return NotFound("Ürün bulunamadı.");

            var userId = int.Parse(User.FindFirstValue("userId")!);

            // Sadece restoran sahibi silebilir
            if (product.Restaurant!.OwnerId != userId)
                return Forbid();

            // Fiziksel silme yerine soft delete yapılır
            product.IsDeleted = true;

            await _context.SaveChangesAsync();

            return Ok("Ürün menüden kaldırıldı.");
        }

        // Ürün arama işlemi
        [HttpGet("ara")]
        public async Task<IActionResult> SearchProducts([FromQuery] string keyword)
        {
            // Boş arama engellenir
            if (string.IsNullOrWhiteSpace(keyword))
                return BadRequest("Lütfen bir arama kelimesi girin.");

            // İsim veya açıklamada arama yapılır
            var products = await _context.Products
                .Where(p => !p.IsDeleted &&
                       (p.Name.Contains(keyword) || p.Description.Contains(keyword)))
                .ToListAsync();

            if (!products.Any())
                return NotFound("Aradığınız kritere uygun yemek bulunamadı.");

            return Ok(products);
        }
    }
}