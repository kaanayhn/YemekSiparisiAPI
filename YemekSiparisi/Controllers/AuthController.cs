// Kullanıcı kayıt olma, giriş yapma, şifre sıfırlama ve rol ekleme işlemleri

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using YemekSiparisi.Data;
using YemekSiparisi.DTOs;
using YemekSiparisi.Models;

namespace YemekSiparisi.Controllers
{
    [Tags("1. Kimlik Doğrulama ve Güvenlik")]

    [Route("api/KimlikDogrulama")]

    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // Kullanıcı kayıt işlemi
        [HttpPost("kayit-ol")]
        public IActionResult Register(UserRegisterDto request)
        {
            // Kullanıcı adı veya email daha önce alınmış mı kontrol edilir
            if (_context.Users.Any(u => u.Username == request.Username || u.Email == request.Email))
            {
                return BadRequest("Bu kullanıcı adı veya e-posta zaten kullanımda.");
            }

            // Customer rolü veritabanından çekilir
            var customerRole = _context.Roles.FirstOrDefault(r => r.RoleName == "Customer");

            // Rol yoksa işlem durdurulur
            if (customerRole == null)
            {
                return BadRequest("Sistemde 'Customer' rolü bulunamadı. Lütfen önce rolleri oluşturun.");
            }

            // Şifre güvenlik için hashlenir
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Yeni kullanıcı oluşturulur
            var newUser = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = passwordHash,
                RoleId = customerRole.Id
            };

            // Kullanıcı veritabanına eklenir
            _context.Users.Add(newUser);
            _context.SaveChanges();

            return Ok("Kullanıcı başarıyla kaydedildi.");
        }

        // Kullanıcı giriş işlemi
        [HttpPost("giris-yap")]
        public IActionResult Login(UserLoginDto request)
        {
            // Kullanıcı bulunur ve Role bilgisi ile birlikte çekilir
            var user = _context.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.Username == request.Username);

            // Kullanıcı yoksa veya şifre yanlışsa giriş reddedilir
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return BadRequest("Hatalı kullanıcı adı veya şifre.");
            }

            // Token oluşturulur
            var token = CreateToken(user);

            return Ok(new { token = token, message = "Giriş başarılı!" });
        }

        // Şifre sıfırlama işlemi
        [HttpPost("sifre-sifirla")]
        public IActionResult ResetPassword(PasswordResetDto request)
        {
            // Kullanıcı bulunur
            var user = _context.Users.FirstOrDefault(u => u.Username == request.Username);

            if (user == null)
                return NotFound("Kullanıcı bulunamadı.");

            // Eski şifre doğrulaması yapılır
            if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.PasswordHash))
            {
                return BadRequest("Eski şifreniz hatalı. Şifre sıfırlama işlemi reddedildi.");
            }

            // Yeni şifre hashlenerek kaydedilir
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            _context.SaveChanges();

            return Ok("Şifreniz başarıyla sıfırlandı.");
        }

        // JWT token oluşturma metodu
        private string CreateToken(User user)
        {
            // Token içine eklenecek bilgiler
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role.RoleName),
                new Claim("userId", user.Id.ToString())
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            // Token oluşturulur
            var tokenDescriptor = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

        // Yeni rol ekleme işlemi (sadece Admin erişebilir)
        [Authorize(Roles = "Admin")]
        [HttpPost("rol-ekle/{roleName}")]
        public IActionResult AddRole(string roleName)
        {
            // Yeni rol oluşturulur
            var newRole = new Role { RoleName = roleName };

            _context.Roles.Add(newRole);
            _context.SaveChanges();

            return Ok(roleName + " rolü başarıyla eklendi.");
        }
    }
}