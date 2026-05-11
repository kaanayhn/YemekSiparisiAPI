namespace YemekSiparisi.DTOs
{
    public class PasswordResetDto
    {
        public string Username { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}