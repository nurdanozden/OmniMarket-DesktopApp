using Microsoft.EntityFrameworkCore;
using OmniMarket.Models;

namespace OmniMarket.Data;

public class AppDbContext : DbContext
{
    public DbSet<Market> Markets { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Log> Logs { get; set; } = null!;
    public DbSet<Tedarikci> Tedarikciler { get; set; } = null!;
    public DbSet<MarketProfil> MarketProfiller { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=OmniMarketDb;Username=postgres;Password=changeme");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Market → Products (1:N)
        modelBuilder.Entity<Product>()
            .HasOne(p => p.Market)
            .WithMany(m => m.Products)
            .HasForeignKey(p => p.MarketId)
            .OnDelete(DeleteBehavior.Cascade);

        // Market → Suppliers (1:N)
        modelBuilder.Entity<Tedarikci>()
            .HasOne(s => s.Market)
            .WithMany(m => m.Tedarikciler)
            .HasForeignKey(s => s.MarketId)
            .OnDelete(DeleteBehavior.Cascade);

        // Supplier → Products (1:N)
        modelBuilder.Entity<Product>()
            .HasOne(p => p.Tedarikci)
            .WithMany(s => s.Products)
            .HasForeignKey(p => p.TedarikciId)
            .OnDelete(DeleteBehavior.SetNull);

        // Market → MarketProfil (1:1)
        modelBuilder.Entity<Market>()
            .HasOne(m => m.MarketProfil)
            .WithOne(p => p.Market)
            .HasForeignKey<MarketProfil>(p => p.MarketId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique username constraint
        modelBuilder.Entity<Market>()
            .HasIndex(m => m.Username)
            .IsUnique();
    }
}
