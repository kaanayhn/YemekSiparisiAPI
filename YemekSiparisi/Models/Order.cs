// Kullanıcının verdiği siparişlerin temel bilgilerini içerir

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YemekSiparisi.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        // Siparişin oluşturulma tarihi
        // Varsayılan olarak sistem zamanı atanır
        public DateTime OrderDate { get; set; } = DateTime.Now;

        // Siparişin toplam tutarı
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        // Siparişin mevcut durumu
        // Varsayılan olarak "Hazırlanıyor" başlar
        public string Status { get; set; } = "Hazırlanıyor";

        // Siparişi veren kullanıcının ID'si
        public int UserId { get; set; }

        public User? User { get; set; }

        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}