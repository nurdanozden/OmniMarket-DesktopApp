using Microsoft.EntityFrameworkCore;
using OmniMarket.Models;

namespace OmniMarket.Data;

public class AppDbContext : DbContext
{
    public DbSet<Market> Markets { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Log> Logs { get; set; } = null!;

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

        // Market → Logs (1:N)
        modelBuilder.Entity<Log>()
            .HasOne(l => l.Market)
            .WithMany()
            .HasForeignKey(l => l.MarketId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique username constraint
        modelBuilder.Entity<Market>()
            .HasIndex(m => m.Username)
            .IsUnique();
    }
}
