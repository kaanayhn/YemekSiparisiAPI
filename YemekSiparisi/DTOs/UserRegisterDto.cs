using System.ComponentModel.DataAnnotations;

namespace YemekSiparisi.DTOs
{
    public class UserRegisterDto
    {
        [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        public string Password { get; set; }

        public int RoleId { get; set; }
    }
}