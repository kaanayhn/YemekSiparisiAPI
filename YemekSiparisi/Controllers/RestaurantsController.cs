// Restoran ekleme, listeleme, güncelleme, silme ve arama işlemleri yapılır

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using YemekSiparisi.Data;
using YemekSiparisi.Models;

namespace YemekSiparisi.Controllers
{
    [Tags("4. Restoran Yönetimi")]

    [Route("api/Restoranlar")]

    [ApiController]
    public class RestaurantsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RestaurantsController(AppDbContext context)
        {
            _context = context;
        }

        // Tüm restoranları listeleme (silinmemiş olanlar)
        [HttpGet]
        public async Task<IActionResult> GetRestaurants()
        {
            var restaurants = await _context.Restaurants
                .Where(r => !r.IsDeleted)
                .ToListAsync();

            return Ok(restaurants);
        }

        // Restoran oluşturma (sadece RestaurantOwner rolü)
        [Authorize(Roles = "RestaurantOwner")]
        [HttpPost]
        public async Task<IActionResult> CreateRestaurant(Restaurant restaurant)
        {
            // Token içinden kullanıcı id alınır
            var userIdClaim = User.FindFirstValue("userId");

            // Token yoksa işlem durdurulur
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("Token içinde kullanıcı ID bulunamadı.");

            // Restoran sahibi atanır
            restaurant.OwnerId = int.Parse(userIdClaim);

            // Navigation döngüsünü engellemek için temizlenir
            restaurant.Owner = null;

            try
            {
                _context.Restaurants.Add(restaurant);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Restoran başarıyla eklendi!",
                    id = restaurant.Id
                });
            }
            catch (Exception ex)
            {
                // Hata olursa detay döner
                return StatusCode(500, $"Veritabanı hatası: {ex.Message}");
            }
        }

        // Restoran silme
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRestaurant(int id)
        {
            var restaurant = await _context.Restaurants.FindAsync(id);

            if (restaurant == null || restaurant.IsDeleted)
                return NotFound("Restoran bulunamadı.");

            var userId = int.Parse(User.FindFirstValue("userId")!);

            // Sadece sahibi silebilir
            if (restaurant.OwnerId != userId)
                return Forbid();

            restaurant.IsDeleted = true;

            await _context.SaveChangesAsync();

            return Ok("Restoran başarıyla silindi (mantıksal olarak).");
        }

        // Tek restoran getir
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRestaurant(int id)
        {
            var restaurant = await _context.Restaurants.FindAsync(id);

            if (restaurant == null || restaurant.IsDeleted)
                return NotFound("Restoran bulunamadı.");

            return Ok(restaurant);
        }

        // Restoran güncelleme
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRestaurant(int id, Restaurant updatedRestaurant)
        {
            var restaurant = await _context.Restaurants.FindAsync(id);

            if (restaurant == null)
                return NotFound("Restoran bulunamadı.");

            var userId = int.Parse(User.FindFirstValue("userId")!);

            // Sadece sahibi güncelleyebilir
            if (restaurant.OwnerId != userId)
                return Forbid();

            restaurant.Name = updatedRestaurant.Name;
            restaurant.LogoUrl = updatedRestaurant.LogoUrl;
            restaurant.Address = updatedRestaurant.Address;
            restaurant.Description = updatedRestaurant.Description;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Restoran başarıyla güncellendi!",
                restaurant
            });
        }

        // Restoran arama
        [HttpGet("ara")]
        public async Task<IActionResult> SearchRestaurants([FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return BadRequest("Lütfen bir arama kelimesi girin.");

            var restaurants = await _context.Restaurants
                .Where(r => !r.IsDeleted &&
                       (r.Name.Contains(keyword) || r.Description.Contains(keyword)))
                .ToListAsync();

            if (!restaurants.Any())
                return NotFound("Aradığınız kritere uygun restoran bulunamadı.");

            return Ok(restaurants);
        }
    }
}