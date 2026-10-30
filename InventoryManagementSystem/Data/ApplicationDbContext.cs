using InventoryManagementSystem.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, Role, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ⭐ NEW: Tenant DbSet
        public DbSet<Tenant> Tenants { get; set; }

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

            modelBuilder.Entity<ApplicationUser>().ToTable("Users");
            modelBuilder.Entity<Role>().ToTable("Roles");
            modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<string>>().ToTable("UserRoles");
            modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserClaim<string>>().ToTable("UserClaims");
            modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserLogin<string>>().ToTable("UserLogins");
            modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserToken<string>>().ToTable("UserTokens");
            modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>>().ToTable("RoleClaims");

            // ============ ⭐ NEW: TENANT ============
            modelBuilder.Entity<Tenant>(entity =>
            {
                entity.HasIndex(t => t.Code).IsUnique();
                entity.Property(t => t.Name).IsRequired().HasMaxLength(100);
                entity.Property(t => t.Code).IsRequired().HasMaxLength(20);
                entity.Property(t => t.Address).HasMaxLength(255);
                entity.Property(t => t.Phone).HasMaxLength(20);
            });

            // ============ ⭐ NEW: USER → TENANT ============
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.HasOne(u => u.Tenant)
                    .WithMany(t => t.Users)
                    .HasForeignKey(u => u.TenantId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ============ CATEGORY ============
            modelBuilder.Entity<Category>(entity =>
            {
                // ⭐ Tenant-specific unique index (was global)
                entity.HasIndex(c => new { c.TenantId, c.Name }).IsUnique();
                entity.Property(c => c.Name).IsRequired().HasMaxLength(100);

                // ⭐ Tenant relationship
                entity.HasOne(c => c.Tenant)
                    .WithMany(t => t.Categories)
                    .HasForeignKey(c => c.TenantId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ============ PRODUCT ============
            modelBuilder.Entity<Product>(entity =>
            {
                // ⭐ Tenant-specific unique index (was global)
                entity.HasIndex(p => new { p.TenantId, p.SKU }).IsUnique();
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

                // ⭐ Tenant relationship
                entity.HasOne(p => p.Tenant)
                    .WithMany(t => t.Products)
                    .HasForeignKey(p => p.TenantId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ============ SUPPLIER ============
            modelBuilder.Entity<Supplier>(entity =>
            {
                entity.Property(s => s.Name).IsRequired().HasMaxLength(100);

                // ⭐ Tenant relationship
                entity.HasOne(s => s.Tenant)
                    .WithMany(t => t.Suppliers)
                    .HasForeignKey(s => s.TenantId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ============ CUSTOMER ============
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.Property(c => c.Name).IsRequired().HasMaxLength(100);

                // ⭐ Tenant relationship
                entity.HasOne(c => c.Tenant)
                    .WithMany(t => t.Customers)
                    .HasForeignKey(c => c.TenantId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ============ PURCHASE ============
            modelBuilder.Entity<Purchase>(entity =>
            {
                // ⭐ Tenant-specific unique index (was global)
                entity.HasIndex(p => new { p.TenantId, p.PurchaseNumber }).IsUnique();
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

                // ⭐ Tenant relationship
                entity.HasOne(p => p.Tenant)
                    .WithMany(t => t.Purchases)
                    .HasForeignKey(p => p.TenantId)
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
                // ⭐ Tenant-specific unique index (was global)
                entity.HasIndex(s => new { s.TenantId, s.InvoiceNumber }).IsUnique();
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

                // ⭐ Tenant relationship
                entity.HasOne(s => s.Tenant)
                    .WithMany(t => t.Sales)
                    .HasForeignKey(s => s.TenantId)
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

                entity.HasOne(st => st.Tenant)
                    .WithMany(t => t.StockTransactions)
                    .HasForeignKey(st => st.TenantId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}