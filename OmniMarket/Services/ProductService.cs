using Microsoft.EntityFrameworkCore;
using System.Windows;
using OmniMarket.Data;
using OmniMarket.Models;

namespace OmniMarket.Services;

public class ProductService
{



    public List<Product> GetProducts(int marketId)
    {
        using var db = new AppDbContext();

        NormalizeExpiredProductSuppliers(db, marketId);

        var rawResults = db.Products
            .Include(p => p.Tedarikci)
            .Where(p => p.MarketId == marketId)
            .ToList();

        return GroupProducts(rawResults);
    }



    public List<Product> SearchProducts(int marketId, string searchText)
    {
        try
        {
            using var db = new AppDbContext();
            
            var rawResults = db.Products
                .Include(p => p.Tedarikci)
                .Where(p => p.MarketId == marketId &&
                            ((p.Name != null && p.Name.ToLower().Contains(searchText.ToLower())) ||
                             (p.Barcode != null && p.Barcode.Contains(searchText))))
                .ToList();

            return GroupProducts(rawResults);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ürün arama DB hatası: {ex.Message}", "Veritabanı Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
            return new List<Product>();
        }
    }



    public List<Product> FilterByCategory(int marketId, string category)
    {
        try
        {
            using var db = new AppDbContext();
            var rawResults = db.Products
                .Include(p => p.Tedarikci)
                .Where(p => p.MarketId == marketId && p.Category == category)
                .ToList();

            return GroupProducts(rawResults);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Kategori filtreleme DB hatası: {ex.Message}", "Veritabanı Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
            return new List<Product>();
        }
    }



    public List<string> GetCategories(int marketId)
    {
        try
        {
            using var db = new AppDbContext();
            return db.Products
                .Where(p => p.MarketId == marketId && p.Category != null)
                .Select(p => p.Category!)
                .Distinct()
                .OrderBy(c => c)
                .ToList();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Kategoriler çekilirken DB hatası: {ex.Message}", "Veritabanı Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
            return new List<string>();
        }
    }



    public Product AddProduct(Product product, string kullaniciAdi)
    {
        try
        {
            using var db = new AppDbContext();
            db.Products.Add(product);
            db.SaveChanges();

            var logService = new LogService();
            logService.AddLog(product.MarketId, kullaniciAdi, LogType.Ekleme, $"'{product.Name}' adlı yeni ürün stoğa eklendi (Stok: {product.Stock}).");

            return product;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ürün eklenirken veritabanı hatası oluştu: {ex.Message}", "Sistem Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
            return product;
        }
    }



    public void UpdateProduct(Product product, string kullaniciAdi)
    {
        try
        {
            using var db = new AppDbContext();
            var existingProduct = db.Products.AsNoTracking().FirstOrDefault(p => p.Id == product.Id);
            string oldStockStr = existingProduct != null ? existingProduct.Stock.ToString() : "?";

            db.Products.Update(product);
            db.SaveChanges();

            var logService = new LogService();
            logService.AddLog(product.MarketId, kullaniciAdi, LogType.Guncelleme, $"'{product.Name}' ürünü güncellendi (Eski Stok: {oldStockStr} -> Yeni Stok: {product.Stock}).");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ürün güncellenirken veritabanı hatası oluştu: {ex.Message}", "Sistem Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }



    public void DeleteProduct(int productId, string kullaniciAdi)
    {
        try
        {
            using var db = new AppDbContext();
            var product = db.Products.Find(productId);
            if (product != null)
            {
                db.Products.Remove(product);
                db.SaveChanges();

                var logService = new LogService();
                logService.AddLog(product.MarketId, kullaniciAdi, LogType.Silme, $"'{product.Name}' adlı ürün sistemden tamamen silindi.");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ürün silinirken veritabanı hatası oluştu: {ex.Message}", "Sistem Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }



    public bool BarcodeExists(int marketId, string barcode)
    {
        using var db = new AppDbContext();
        return db.Products.Any(p => p.MarketId == marketId && p.Barcode == barcode);
    }



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



    public int ApplyCampaign(int marketId, decimal discountRate)
    {
        using var db = new AppDbContext();
        var today = DateTime.Today;
        var expiring = db.Products
            .Where(p => p.MarketId == marketId
                     && p.ExpiryDate >= today
                     && (p.ExpiryDate - today).TotalDays <= 7)
            .ToList();

        foreach (var p in expiring)
            p.DiscountRate = discountRate;

        db.SaveChanges();
        return expiring.Count;
    }



    public int RequestReturn(int marketId)
    {
        using var db = new AppDbContext();
        var today = DateTime.Today;
        var expiring = db.Products
            .Where(p => p.MarketId == marketId
                     && p.ExpiryDate >= today
                     && (p.ExpiryDate - today).TotalDays <= 7)
            .ToList();

        foreach (var p in expiring)
            p.ReturnRequested = true;

        db.SaveChanges();
        return expiring.Count;
    }



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




    private List<Product> GroupProducts(List<Product> rawResults)
    {
        if (rawResults == null || !rawResults.Any()) return new List<Product>();

        try
        {
            return rawResults
                .GroupBy(p => p.Name)
                .Select(g => 
                {
                    var first = g.OrderBy(p => p.ExpiryDate).FirstOrDefault();
                    if (first == null) return null;

                    return new Product
                    {
                        Id = first.Id,
                        Name = g.Key ?? "Bilinmiyor",
                        Category = first.Category ?? "",
                        Barcode = first.Barcode ?? "",
                        Stock = g.Sum(p => p.Stock),
                        PurchasePrice = first.PurchasePrice,
                        SalePrice = first.SalePrice,
                        ExpiryDate = first.ExpiryDate,
                        MarketId = first.MarketId,
                        TedarikciId = first.TedarikciId,
                        Tedarikci = first.Tedarikci
                    };
                })
                .Where(p => p != null)
                .Select(p => p!)
                .OrderBy(p => p.ExpiryDate)
                .ToList();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Product Service Grouping Hatası: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}",
                "Crtical Exception in Grouping", MessageBoxButton.OK, MessageBoxImage.Error);
            return new List<Product>();
        }
    }



    private static void NormalizeExpiredProductSuppliers(AppDbContext db, int marketId)
    {
        var suppliers = db.Tedarikciler
            .Where(s => s.MarketId == marketId)
            .ToList();

        if (!suppliers.Any())
            return;

        var supplierById = suppliers.ToDictionary(s => s.Id);
        var defaultSupplier = suppliers.FirstOrDefault(s => s.Kategori == "Genel") ?? suppliers.First();
        var todayUtc = DateTime.UtcNow.Date;

        var expiredProducts = db.Products
            .Where(p => p.MarketId == marketId && p.ExpiryDate < todayUtc)
            .ToList();

        var hasChanges = false;

        foreach (var product in expiredProducts)
        {
            var expectedSupplierCategory = GetExpectedSupplierCategory(product.Category);

            supplierById.TryGetValue(product.TedarikciId ?? -1, out var currentSupplier);

            var isMissingOrInvalid = product.TedarikciId == null || currentSupplier == null;
            var isCategoryMismatch = !string.IsNullOrWhiteSpace(expectedSupplierCategory)
                                     && currentSupplier != null
                                     && currentSupplier.Kategori != expectedSupplierCategory;

            if (!isMissingOrInvalid && !isCategoryMismatch)
                continue;

            var preferredSupplier = !string.IsNullOrWhiteSpace(expectedSupplierCategory)
                ? suppliers.FirstOrDefault(s => s.Kategori == expectedSupplierCategory)
                : null;

            product.TedarikciId = (preferredSupplier ?? defaultSupplier).Id;
            hasChanges = true;
        }

        if (hasChanges)
            db.SaveChanges();
    }

    private static string? GetExpectedSupplierCategory(string productCategory)
    {
        return productCategory switch
        {
            "Süt" => "Sütçü",
            "Kahvaltılık" => "Sütçü",
            "Süt & Süt Ürünleri" => "Sütçü",
            "Meyve & Sebze" => "Manav",
            "Dondurulmuş Gıda" => "Soğuk Zincir",
            "Dondurulmuş Gıdalar" => "Soğuk Zincir",
            "Bakliyat" => "Bakliyatçı",
            "Un & Bakliyat" => "Bakliyatçı",
            _ => null
        };
    }
}

