using System.ComponentModel.DataAnnotations;

namespace YemekSiparisi.Models
{
    public class Role
    {
        [Key]
        public int Id { get; set; }
        public string RoleName { get; set; } // Admin, RestaurantOwner, Customer
    }
}