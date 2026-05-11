
using Microsoft.EntityFrameworkCore;
using YemekSiparisi.Models;

namespace YemekSiparisi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Veritabanı tabloları
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Restaurant> Restaurants { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        // Veritabanı model ilişkileri ve başlangıç ayarları burada yapılır
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.NoAction);

            // Veritabanı ilk oluşturulduğunda sistem rolleri otomatik eklenir
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, RoleName = "Admin" },
                new Role { Id = 2, RoleName = "RestaurantOwner" },
                new Role { Id = 3, RoleName = "Customer" }
            );
        }
    }
}