
using Microsoft.AspNetCore.Mvc;
using YemekSiparisi.DTOs;

namespace YemekSiparisi.Controllers
{
    [Tags("5. Ödeme Sistemi")]

    [Route("api/OdemeSistemi")]

    [ApiController]
    public class PaymentController : ControllerBase
    {
        // Ödeme işlemi endpointi
        [HttpPost("odeme-yap")]
        public IActionResult ProcessPayment(PaymentDto payment)
        {
            // Kart numarası 16 haneli mi kontrol edilir
            // CVV 3 haneli mi kontrol edilir
            // Tutar 0’dan büyük mü kontrol edilir
            if (payment.CardNumber != null && payment.CardNumber.Length == 16 &&
                payment.CVV != null && payment.CVV.Length == 3 &&
                payment.Amount > 0)
            {
                return Ok(new
                {
                    status = "Success",
                    message = "Ödeme onaylandı. Siparişiniz hazırlanıyor!"
                });
            }

            // Eğer şartlar sağlanmazsa ödeme reddedilir
            return BadRequest(new
            {
                status = "Failed",
                message = "Ödeme reddedildi. Kart bilgilerinizi ve tutarı kontrol edin."
            });
        }
    }
}