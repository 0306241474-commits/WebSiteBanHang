using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using WebBanHang.Models;

namespace WebBanHang.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Category>()
                .ToTable("Category");

            builder.Entity<Product>()
                .ToTable("Product");

            builder.Entity<Order>()
                .ToTable("Order");

            builder.Entity<Order>()
                .Property(order => order.Status)
                .HasMaxLength(50);

            builder.Entity<OrderItem>()
                .ToTable("OrderDetail");

            builder.Entity<OrderItem>()
                .Property(item => item.UnitPrice)
                .HasColumnType("decimal(18,2)");

            builder.Entity<OrderItem>()
                .HasOne(item => item.Product)
                .WithMany()
                .HasForeignKey(item => item.ProductId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}