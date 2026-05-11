
using System.ComponentModel.DataAnnotations;

namespace YemekSiparisi.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
        [StringLength(50)]
        public string Username { get; set; }

        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta giriniz.")]
        public string Email { get; set; }

        // Şifre güvenlik nedeniyle hashlenmiş şekilde saklanır
        public string PasswordHash { get; set; }

        // Kullanıcının rolünü belirleyen foreign key
        public int RoleId { get; set; }

        // Navigation property (kullanıcının rol bilgisine erişmek için)
        public Role Role { get; set; }
    }
}