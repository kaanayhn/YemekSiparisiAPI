
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YemekSiparisi.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Ürün adı zorunludur.")]
        [StringLength(100, ErrorMessage = "Ürün adı en fazla 100 karakter olabilir.")]
        public string Name { get; set; }

        // Ürün açıklaması
        public string Description { get; set; }

        // Ürün fiyatı
        // 0.01 ile 100000 arasında olmalıdır
        [Required]
        [Range(0.01, 100000, ErrorMessage = "Fiyat 0'dan büyük olmalıdır.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        // Ürün stok bilgisi
        // 0 ile 10000 arasında olmalıdır
        [Required]
        [Range(0, 10000, ErrorMessage = "Stok eksiye düşemez.")]
        public int Stock { get; set; }

        public bool IsDeleted { get; set; } = false;

        // Ürün görsel URL bilgisi
        public string ImageUrl { get; set; }

        // Ürünün bağlı olduğu restoranın ID’si
        public int RestaurantId { get; set; }

        // Navigation property (ürünün hangi restorana ait olduğunu gösterir)
        public Restaurant? Restaurant { get; set; }
    }
}