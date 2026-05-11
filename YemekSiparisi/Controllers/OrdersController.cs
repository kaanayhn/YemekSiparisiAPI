// Sipariş oluşturma, geçmiş siparişleri görme ve sipariş durumunu güncelleme işlemleri burada yapılır

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using YemekSiparisi.Data;
using YemekSiparisi.DTOs;
using YemekSiparisi.Models;

namespace YemekSiparisi.Controllers
{
    [Tags("2. Sipariş İşlemleri")]

    [Route("api/Siparisler")]

    [ApiController]

    // Bu controller içindeki tüm işlemler için giriş zorunlu
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        // Sipariş oluşturma işlemi
        [HttpPost]
        public async Task<IActionResult> PlaceOrder(List<OrderItemDto> items)
        {
            // Sepet boş mu kontrol edilir
            if (items == null || !items.Any())
                return BadRequest("Sepetiniz boş! Lütfen önce yemek ekleyin.");

            // Token içinden kullanıcı id alınır
            var userId = int.Parse(User.FindFirstValue("userId")!);

            // Yeni sipariş oluşturulur
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                Status = "Hazırlanıyor",
                TotalAmount = 0
            };

            // Sepetteki her ürün işlenir
            foreach (var item in items)
            {
                // Ürün veritabanından bulunur
                var product = await _context.Products.FindAsync(item.ProductId);

                if (product == null)
                    return BadRequest("Yemek bulunamadı.");

                // Stok kontrolü yapılır
                if (product.Stock < item.Quantity)
                    return BadRequest($"{product.Name} adlı üründen yeterli stok yok. Kalan stok: {product.Stock}");

                // Stok düşülür
                product.Stock -= item.Quantity;

                // Sipariş kalemi oluşturulur
                var orderItem = new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = product.Price
                };

                // Toplam tutar hesaplanır
                order.TotalAmount += (orderItem.Price * orderItem.Quantity);

                // Siparişe eklenir
                order.OrderItems.Add(orderItem);
            }

            // Sipariş veritabanına kaydedilir
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Siparişiniz başarıyla alındı!",
                total = order.TotalAmount,
                status = order.Status
            });
        }

        // Kullanıcının geçmiş siparişlerini getirir
        [HttpGet("gecmis-siparislerim")]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = int.Parse(User.FindFirstValue("userId")!);

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return Ok(orders);
        }

        // Tek bir siparişi getirir
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var userId = int.Parse(User.FindFirstValue("userId")!);

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound("Sipariş bulunamadı.");

            // Kullanıcı kendi siparişini görebilir
            if (order.UserId != userId)
                return Forbid();

            return Ok(order);
        }

        // Sipariş durumunu güncelleme (Admin veya RestaurantOwner)
        [Authorize(Roles = "Admin,RestaurantOwner")]
        [HttpPatch("{id}/durumu-guncelle")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromQuery] string newStatus)
        {
            // Sipariş ürünleri ve restoran bilgisiyle birlikte çekilir
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.Restaurant)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound("Sipariş bulunamadı.");

            var userId = int.Parse(User.FindFirstValue("userId")!);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            // Admin değilse sadece kendi restoranının siparişlerini güncelleyebilir
            if (userRole != "Admin")
            {
                var isMyOrder = order.OrderItems.Any(oi =>
                    oi.Product.Restaurant != null &&
                    oi.Product.Restaurant.OwnerId == userId);

                if (!isMyOrder)
                    return Forbid("Sadece kendi restoranınıza ait siparişlerin durumunu güncelleyebilirsiniz.");
            }

            // Geçerli durumlar kontrol edilir
            var validStatuses = new List<string>
            {
                "Hazırlanıyor",
                "Yolda",
                "Teslim Edildi"
            };

            if (!validStatuses.Contains(newStatus))
                return BadRequest("Geçersiz durum!");

            // Sipariş durumu güncellenir
            order.Status = newStatus;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Sipariş durumu '{newStatus}' olarak güncellendi."
            });
        }
    }
}