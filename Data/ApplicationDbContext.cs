using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using web_lab_4.Models;

namespace web_lab_4.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ignore session-only models (không lưu vào database)
            modelBuilder.Ignore<CartItem>();
            modelBuilder.Ignore<ShoppingCart>();

            // Configure Category
            modelBuilder.Entity<Category>()
                .HasMany(c => c.Products)
                .WithOne(p => p.Category)
                .HasForeignKey(p => p.CategoryId);

            // Configure Product
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasMany(p => p.Images)
                    .WithOne(pi => pi.Product)
                    .HasForeignKey(pi => pi.ProductId);

                // Configure decimal precision for weight and price
                entity.Property(p => p.Price)
                    .HasColumnType("decimal(18,2)");
                
                entity.Property(p => p.Weight)
                    .HasColumnType("decimal(10,3)"); // Cho phép 3 chữ số thập phân cho trọng lượng
                
                // Set default values
                entity.Property(p => p.StockQuantity)
                    .HasDefaultValue(0);
                
                entity.Property(p => p.WeightUnit)
                    .HasDefaultValue("g");
                
                entity.Property(p => p.IsAvailable)
                    .HasDefaultValue(true);
            });

            // Configure Order
            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderDetails)
                .WithOne(od => od.Order)
                .HasForeignKey(od => od.OrderId);

            // Configure OrderDetail
            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.HasOne(od => od.Product)
                    .WithMany()
                    .HasForeignKey(od => od.ProductId);
                
                // Configure decimal precision
                entity.Property(od => od.Price)
                    .HasColumnType("decimal(18,2)");
                
                entity.Property(od => od.Weight)
                    .HasColumnType("decimal(10,3)");
                
                // Set default values
                entity.Property(od => od.WeightUnit)
                    .HasDefaultValue("g");
            });

            // Configure Order-User relationship
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Review configuration
            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasKey(r => r.Id);
                
                entity.Property(r => r.Rating).IsRequired();
                entity.Property(r => r.CreatedAt).IsRequired();
                entity.Property(r => r.ReviewerName).IsRequired().HasMaxLength(100);
                entity.Property(r => r.Comment).HasMaxLength(1000);
                
                entity.HasOne(r => r.Product)
                      .WithMany(p => p.Reviews)
                      .HasForeignKey(r => r.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.User)
                      .WithMany()
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Ensure one review per user per product
                entity.HasIndex(r => new { r.UserId, r.ProductId }).IsUnique();
            });
        }
    }
}