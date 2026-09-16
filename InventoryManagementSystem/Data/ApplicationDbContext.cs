using InventoryManagementSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ============ DbSets ============
        public DbSet<Role> Roles { get; set; }
        public DbSet<ApplicationUser> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<PurchaseItem> PurchaseItems { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleItem> SaleItems { get; set; }
        public DbSet<StockTransaction> StockTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ============ ROLE ============
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasIndex(r => r.Name).IsUnique();
                entity.Property(r => r.Name).IsRequired().HasMaxLength(50);
            });

            // ============ USER ============
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Email).IsRequired().HasMaxLength(150);
                entity.Property(u => u.FullName).IsRequired().HasMaxLength(100);
                entity.Property(u => u.PasswordHash).IsRequired();

                entity.HasOne(u => u.Role)
                    .WithMany(r => r.Users)
                    .HasForeignKey(u => u.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ============ CATEGORY ============
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasIndex(c => c.Name).IsUnique();
                entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
            });

            // ============ PRODUCT ============
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasIndex(p => p.SKU).IsUnique();
                entity.Property(p => p.SKU).IsRequired().HasMaxLength(50);
                entity.Property(p => p.Price).HasPrecision(18, 2);
                entity.Property(p => p.CostPrice).HasPrecision(18, 2);

                entity.HasOne(p => p.Category)
                    .WithMany(c => c.Products)
                    .HasForeignKey(p => p.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Supplier)
                    .WithMany(s => s.Products)
                    .HasForeignKey(p => p.SupplierId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ============ SUPPLIER ============
            modelBuilder.Entity<Supplier>(entity =>
            {
                entity.Property(s => s.Name).IsRequired().HasMaxLength(100);
                entity.HasIndex(s => s.Email).IsUnique().HasFilter("[Email] IS NOT NULL");
            });

            // ============ CUSTOMER ============
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
            });

            // ============ PURCHASE ============
            modelBuilder.Entity<Purchase>(entity =>
            {
                entity.HasIndex(p => p.PurchaseNumber).IsUnique();
                entity.Property(p => p.PurchaseNumber).IsRequired().HasMaxLength(50);
                entity.Property(p => p.TotalAmount).HasPrecision(18, 2);

                entity.HasOne(p => p.Supplier)
                    .WithMany(s => s.Purchases)
                    .HasForeignKey(p => p.SupplierId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.User)
                    .WithMany(u => u.Purchases)
                    .HasForeignKey(p => p.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ============ PURCHASE ITEM ============
            modelBuilder.Entity<PurchaseItem>(entity =>
            {
                entity.Property(pi => pi.UnitPrice).HasPrecision(18, 2);
                entity.Property(pi => pi.SubTotal).HasPrecision(18, 2);

                entity.HasOne(pi => pi.Purchase)
                    .WithMany(p => p.PurchaseItems)
                    .HasForeignKey(pi => pi.PurchaseId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pi => pi.Product)
                    .WithMany(p => p.PurchaseItems)
                    .HasForeignKey(pi => pi.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ============ SALE ============
            modelBuilder.Entity<Sale>(entity =>
            {
                entity.HasIndex(s => s.InvoiceNumber).IsUnique();
                entity.Property(s => s.InvoiceNumber).IsRequired().HasMaxLength(50);
                entity.Property(s => s.TotalAmount).HasPrecision(18, 2);
                entity.Property(s => s.Discount).HasPrecision(18, 2);

                entity.HasOne(s => s.Customer)
                    .WithMany(c => c.Sales)
                    .HasForeignKey(s => s.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.User)
                    .WithMany(u => u.Sales)
                    .HasForeignKey(s => s.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ============ SALE ITEM ============
            modelBuilder.Entity<SaleItem>(entity =>
            {
                entity.Property(si => si.UnitPrice).HasPrecision(18, 2);
                entity.Property(si => si.SubTotal).HasPrecision(18, 2);

                entity.HasOne(si => si.Sale)
                    .WithMany(s => s.SaleItems)
                    .HasForeignKey(si => si.SaleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(si => si.Product)
                    .WithMany(p => p.SaleItems)
                    .HasForeignKey(si => si.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ============ STOCK TRANSACTION ============
            modelBuilder.Entity<StockTransaction>(entity =>
            {
                entity.Property(st => st.Type).IsRequired().HasMaxLength(10);

                entity.HasOne(st => st.Product)
                    .WithMany(p => p.StockTransactions)
                    .HasForeignKey(st => st.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(st => st.User)
                    .WithMany(u => u.StockTransactions)
                    .HasForeignKey(st => st.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}