using Microsoft.EntityFrameworkCore;
using RestaurantPosEnterprise.Domain.Entities;

namespace RestaurantPosEnterprise.Infrastructure.Data;

public class RestaurantDbContext(DbContextOptions<RestaurantDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<DiningTable> Tables => Set<DiningTable>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.ToTable("Users");
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Username).HasMaxLength(60).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Role).HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.HasIndex(x => x.Username).IsUnique();
        });

        modelBuilder.Entity<DiningTable>(entity =>
        {
            entity.ToTable("Tables");
            entity.Property(x => x.Name).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Seats).HasDefaultValue(4);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            entity.HasOne(x => x.AssignedWaiter)
                .WithMany(x => x.AssignedTables)
                .HasForeignKey(x => x.AssignedWaiterId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Category).HasMaxLength(60).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.ImageUrl).HasMaxLength(500);
            entity.Property(x => x.Price).HasColumnType("decimal(10,2)");
            entity.Property(x => x.SizeOptionsJson).HasColumnType("nvarchar(max)");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("Orders");
            entity.Property(x => x.CreatedAtUtc).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.Notes).HasMaxLength(500);
            entity.HasOne(x => x.Table)
                .WithMany(x => x.Orders)
                .HasForeignKey(x => x.TableId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CreatedByUser)
                .WithMany()
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("OrderItems");
            entity.Property(x => x.UnitPrice).HasColumnType("decimal(10,2)");
            entity.Property(x => x.ProductNameSnapshot).HasMaxLength(150).IsRequired();
            entity.Property(x => x.SelectedSize).HasMaxLength(20);
            entity.HasOne(x => x.Order)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.OrderId);
            entity.HasOne(x => x.Product)
                .WithMany(x => x.OrderItems)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
