using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TableFlow.Api.Data.Entities;

namespace TableFlow.Api.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<MenuItemVarient> MenuItemVarients { get; set; }
        public DbSet<Table> Tables { get; set; }
        public DbSet<TableSession> TableSessions { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // configure entities properties

            modelBuilder.Entity<MenuItem>()
                .Property(m => m.BasePrice)
                .HasPrecision(10, 2);

            modelBuilder.Entity<MenuItemVarient>()
                .Property(v => v.Price)
                .HasPrecision(10, 2);

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasPrecision(10, 2);

            modelBuilder.Entity<TableSession>()
                .Property(ts => ts.TotalAmount)
                .HasPrecision(10, 2);

            // configure relationship

            modelBuilder.Entity<TableSession>()
                .HasOne(ts => ts.CreatedBy)
                .WithMany(u => u.TableSessions)
                .HasForeignKey(ts => ts.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MenuItem>()
                .HasOne(m => m.Category)
                .WithMany(c => c.MenuItems)
                .HasForeignKey(m => m.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MenuItemVarient>()
                .HasOne(v => v.MenuItem)
                .WithMany(m => m.MenuItemVarients)
                .HasForeignKey(v => v.MenuItemId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.MenuItem)
                .WithMany(m => m.OrderItems)
                .HasForeignKey(oi => oi.MenuItemId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Varient)
                .WithMany(v => v.OrderItems)
                .HasForeignKey(oi => oi.VarientId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.TableSession)
                .WithMany(s => s.Orders)
                .HasForeignKey(o => o.SessionId);

            // indexing for database query

            modelBuilder.Entity<TableSession>()
                .HasIndex(ts => ts.TableId);

            modelBuilder.Entity<TableSession>()
                .HasIndex(ts => ts.SessionStatus);

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.SessionId);

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.OrderStatus);

            modelBuilder.Entity<OrderItem>()
                .HasIndex(oi => oi.OrderId);

            modelBuilder.Entity<OrderItem>()
                .HasIndex(oi => oi.OrderItemStatus);

            modelBuilder.Entity<MenuItem>()
                .HasIndex(m => m.CategoryId);

            modelBuilder.Entity<MenuItem>()
                .HasIndex(m => m.IsAvailable);

            modelBuilder.Entity<TableSession>()
                .HasIndex(ts => new { ts.TableId, ts.SessionStatus });
        }
    }
}
