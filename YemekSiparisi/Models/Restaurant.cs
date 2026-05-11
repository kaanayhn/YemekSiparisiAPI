
using System.ComponentModel.DataAnnotations;

namespace YemekSiparisi.Models
{
    public class Restaurant
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Restoran adı zorunludur.")]
        [StringLength(100)]
        public string Name { get; set; }

        public string LogoUrl { get; set; }

        [Required(ErrorMessage = "Adres zorunludur.")]
        [StringLength(250)]
        public string Address { get; set; }

        // Restoran puanı (rating)
        public double Rating { get; set; }

        // Açıklama bilgisi
        public string Description { get; set; }

        public bool IsDeleted { get; set; } = false;

        public int OwnerId { get; set; }

        // Navigation property (restoran sahibine erişim için)
        public User? Owner { get; set; }
    }
}