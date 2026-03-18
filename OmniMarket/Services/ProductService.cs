using Microsoft.EntityFrameworkCore;
using OmniMarket.Data;
using OmniMarket.Models;

namespace OmniMarket.Services;

public class ProductService
{
    /// <summary>
    /// Belirli bir markete ait tüm ürünleri getirir.
    /// </summary>
    public List<Product> GetProducts(int marketId)
    {
        using var db = new AppDbContext();
        return db.Products
            .Where(p => p.MarketId == marketId)
            .OrderBy(p => p.ExpiryDate)
            .ToList();
    }

    /// <summary>
    /// Ürün adına göre arama yapar.
    /// </summary>
    public List<Product> SearchProducts(int marketId, string searchText)
    {
        using var db = new AppDbContext();
        return db.Products
            .Where(p => p.MarketId == marketId &&
                        (p.Name.ToLower().Contains(searchText.ToLower()) ||
                         p.Barcode.Contains(searchText)))
            .OrderBy(p => p.ExpiryDate)
            .ToList();
    }

    /// <summary>
    /// Kategoriye göre filtreler.
    /// </summary>
    public List<Product> FilterByCategory(int marketId, string category)
    {
        using var db = new AppDbContext();
        return db.Products
            .Where(p => p.MarketId == marketId && p.Category == category)
            .OrderBy(p => p.ExpiryDate)
            .ToList();
    }

    /// <summary>
    /// Marketteki tüm kategorileri getirir.
    /// </summary>
    public List<string> GetCategories(int marketId)
    {
        using var db = new AppDbContext();
        return db.Products
            .Where(p => p.MarketId == marketId)
            .Select(p => p.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToList();
    }

    /// <summary>
    /// Yeni ürün ekler.
    /// </summary>
    public Product AddProduct(Product product)
    {
        using var db = new AppDbContext();
        db.Products.Add(product);
        db.SaveChanges();
        return product;
    }

    /// <summary>
    /// Mevcut ürünü günceller.
    /// </summary>
    public void UpdateProduct(Product product)
    {
        using var db = new AppDbContext();
        db.Products.Update(product);
        db.SaveChanges();
    }

    /// <summary>
    /// Ürünü siler.
    /// </summary>
    public void DeleteProduct(int productId)
    {
        using var db = new AppDbContext();
        var product = db.Products.Find(productId);
        if (product != null)
        {
            db.Products.Remove(product);
            db.SaveChanges();
        }
    }

    /// <summary>
    /// Barkodun bu markette mevcut olup olmadığını kontrol eder.
    /// </summary>
    public bool BarcodeExists(int marketId, string barcode)
    {
        using var db = new AppDbContext();
        return db.Products.Any(p => p.MarketId == marketId && p.Barcode == barcode);
    }

    /// <summary>
    /// Dashboard istatistiklerini hesaplar.
    /// </summary>
    public (int TotalProducts, int ExpiringProducts, int ExpiredProducts, decimal TotalStockValue) GetDashboardStats(int marketId)
    {
        using var db = new AppDbContext();
        var products = db.Products.Where(p => p.MarketId == marketId).ToList();

        var total = products.Count;
        var expired = products.Count(p => DateTime.Today > p.ExpiryDate);
        var expiring = products.Count(p => DateTime.Today <= p.ExpiryDate && (p.ExpiryDate - DateTime.Today).TotalDays <= 7);
        var stockValue = products.Sum(p => p.SalePrice * p.Stock);

        return (total, expiring, expired, stockValue);
    }

    /// <summary>
    /// Uyarı listesini getirir.
    /// </summary>
    public List<(string Message, string Type)> GetAlerts(int marketId)
    {
        using var db = new AppDbContext();
        var products = db.Products.Where(p => p.MarketId == marketId).ToList();
        var alerts = new List<(string, string)>();

        foreach (var p in products)
        {
            if (DateTime.Today > p.ExpiryDate)
                alerts.Add(($"⚠ \"{p.Name}\" ürününün son kullanma tarihi geçmiş!", "Expired"));
            else if ((p.ExpiryDate - DateTime.Today).TotalDays <= 7)
                alerts.Add(($"⏰ \"{p.Name}\" ürününün SKT'si yaklaşıyor ({p.ExpiryDate:dd.MM.yyyy})", "Warning"));

            if (p.Stock < 5)
                alerts.Add(($"📦 \"{p.Name}\" stok kritik seviyede ({p.Stock} adet)", "LowStock"));
        }

        return alerts;
    }
}
