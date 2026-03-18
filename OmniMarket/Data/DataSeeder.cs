using OmniMarket.Models;

namespace OmniMarket.Data;

public static class DataSeeder
{
    private static readonly Random _rng = new();

    public static void Seed(AppDbContext context)
    {
        if (context.Markets.Any()) return;

        // ─── Marketler ───
        var nurMarket = new Market
        {
            Name = "NUR MARKET",
            Username = "nur",
            Password = "1234"
        };

        var demoMarket = new Market
        {
            Name = "DEMO MARKET",
            Username = "demo",
            Password = "1234"
        };

        context.Markets.Add(nurMarket);
        context.Markets.Add(demoMarket);
        context.SaveChanges();

        // ─── Ürünler ───
        var productTemplates = GetProductTemplates();

        GenerateProducts(context, nurMarket.Id, productTemplates, 35);
        GenerateProducts(context, demoMarket.Id, productTemplates, 32);

        context.SaveChanges();
    }

    private static void GenerateProducts(AppDbContext context, int marketId,
        List<(string Name, string Category)> templates, int count)
    {
        var shuffled = templates.OrderBy(_ => _rng.Next()).Take(count).ToList();

        for (int i = 0; i < shuffled.Count; i++)
        {
            var (name, category) = shuffled[i];
            var purchasePrice = Math.Round((decimal)(_rng.NextDouble() * 95 + 5), 2);
            var markup = 1.0m + (decimal)(_rng.NextDouble() * 0.3 + 0.1); // %10-%40
            var salePrice = Math.Round(purchasePrice * markup, 2);

            var product = new Product
            {
                Name = name,
                Category = category,
                Barcode = GenerateBarcode(),
                Stock = GetRandomStock(i, shuffled.Count),
                PurchasePrice = purchasePrice,
                SalePrice = salePrice,
                ExpiryDate = DateTime.SpecifyKind(GetRandomExpiryDate(i, shuffled.Count), DateTimeKind.Utc),
                MarketId = marketId
            };

            context.Products.Add(product);
        }
    }

    /// <summary>
    /// SKT dağılımı: %25 geçmiş, %35 yaklaşan, %40 normal
    /// </summary>
    private static DateTime GetRandomExpiryDate(int index, int total)
    {
        double ratio = (double)index / total;

        if (ratio < 0.25)
        {
            // 🔴 SKT geçmiş (1-30 gün önce)
            return DateTime.Today.AddDays(-_rng.Next(1, 31));
        }
        else if (ratio < 0.60)
        {
            // 🟡 SKT yaklaşıyor (bugün + 1-7 gün)
            return DateTime.Today.AddDays(_rng.Next(1, 8));
        }
        else
        {
            // 🟢 Normal (bugün + 8-60 gün)
            return DateTime.Today.AddDays(_rng.Next(8, 61));
        }
    }

    /// <summary>
    /// Stok dağılımı: %20 kritik(1-5), %50 orta(6-20), %30 yüksek(20-100)
    /// </summary>
    private static int GetRandomStock(int index, int total)
    {
        double ratio = (double)index / total;

        if (ratio < 0.20)
            return _rng.Next(1, 6);       // Kritik
        else if (ratio < 0.70)
            return _rng.Next(6, 21);      // Orta
        else
            return _rng.Next(20, 101);    // Yüksek
    }

    private static string GenerateBarcode()
    {
        return $"869{_rng.Next(1000000, 9999999)}";
    }

    private static List<(string Name, string Category)> GetProductTemplates()
    {
        return new List<(string, string)>
        {
            // Un
            ("1kg Çok Amaçlı Un", "Un"),
            ("2kg Ekmek Unu", "Un"),
            ("500g Galeta Unu", "Un"),
            ("1kg Tam Buğday Unu", "Un"),
            ("1kg Mısır Unu", "Un"),
            ("500g Çavdar Unu", "Un"),

            // Süt
            ("1L Günlük Süt", "Süt"),
            ("500ml Yarım Yağlı Süt", "Süt"),
            ("1L Laktozsuz Süt", "Süt"),
            ("200g Beyaz Peynir", "Süt"),
            ("200g Kaşar Peynir", "Süt"),
            ("500g Yoğurt", "Süt"),
            ("1kg Süzme Yoğurt", "Süt"),
            ("200ml Krema", "Süt"),

            // İçecek
            ("330ml Kola", "İçecek"),
            ("1L Portakal Suyu", "İçecek"),
            ("500ml Elma Suyu", "İçecek"),
            ("1.5L Maden Suyu", "İçecek"),
            ("330ml Limonata", "İçecek"),
            ("500ml Ayran", "İçecek"),
            ("250ml Enerji İçeceği", "İçecek"),
            ("1L Şeftali Çayı", "İçecek"),

            // Atıştırmalık
            ("150g Patates Cipsi", "Atıştırmalık"),
            ("100g Çikolata", "Atıştırmalık"),
            ("200g Bisküvi", "Atıştırmalık"),
            ("250g Kuruyemiş Karışım", "Atıştırmalık"),
            ("80g Gofret", "Atıştırmalık"),
            ("300g Kraker", "Atıştırmalık"),
            ("120g Kek", "Atıştırmalık"),

            // Temizlik
            ("1L Bulaşık Deterjanı", "Temizlik"),
            ("3L Çamaşır Deterjanı", "Temizlik"),
            ("750ml Yüzey Temizleyici", "Temizlik"),
            ("1L Cam Sil", "Temizlik"),
            ("500ml El Sabunu", "Temizlik"),
            ("4lü Tuvalet Kağıdı", "Temizlik"),

            // Bakliyat
            ("1kg Kırmızı Mercimek", "Bakliyat"),
            ("1kg Pirinç", "Bakliyat"),
            ("1kg Bulgur", "Bakliyat"),
            ("500g Nohut", "Bakliyat"),
            ("500g Kuru Fasulye", "Bakliyat"),
            ("500g Yeşil Mercimek", "Bakliyat"),
            ("1kg Makarna", "Bakliyat"),

            // Dondurulmuş Gıda
            ("400g Dondurulmuş Bezelye", "Dondurulmuş Gıda"),
            ("500g Dondurulmuş Patates", "Dondurulmuş Gıda"),
            ("450g Dondurulmuş Pizza", "Dondurulmuş Gıda"),
            ("300g Dondurulmuş Börek", "Dondurulmuş Gıda"),
            ("250g Dondurulmuş Karides", "Dondurulmuş Gıda"),
            ("1L Dondurma", "Dondurulmuş Gıda"),
            ("500g Dondurulmuş Sebze Karışımı", "Dondurulmuş Gıda"),
        };
    }
}
