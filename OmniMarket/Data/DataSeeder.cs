using OmniMarket.Models;

namespace OmniMarket.Data;

public static class DataSeeder
{
    private static readonly Random _rng = new();

    public static void Seed(AppDbContext context)
    {
        if (!context.Markets.Any())
        {
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
            var supplierTemplates = GetSupplierTemplates();

            var nurSuppliers = CreateSuppliers(context, nurMarket.Id, supplierTemplates);
            var demoSuppliers = CreateSuppliers(context, demoMarket.Id, supplierTemplates);

            var nurDefaultSupplier = EnsureDefaultSupplier(context, nurMarket.Id, nurSuppliers);
            var demoDefaultSupplier = EnsureDefaultSupplier(context, demoMarket.Id, demoSuppliers);

            context.MarketProfiller.AddRange(
                new MarketProfil
                {
                    MarketId = nurMarket.Id,
                    MarketAdi = nurMarket.Name,
                    Adres = "Merkez Mah. 12. Sokak No: 5",
                    LogoPath = string.Empty,
                    TemaRengi = "#3B82F6"
                },
                new MarketProfil
                {
                    MarketId = demoMarket.Id,
                    MarketAdi = demoMarket.Name,
                    Adres = "Cumhuriyet Cad. No: 20",
                    LogoPath = string.Empty,
                    TemaRengi = "#10B981"
                });

            context.SaveChanges();

            GenerateProducts(context, nurMarket.Id, productTemplates, 360, nurSuppliers, nurDefaultSupplier);
            GenerateProducts(context, demoMarket.Id, productTemplates, 320, demoSuppliers, demoDefaultSupplier);

            context.SaveChanges();

            FixMissingSuppliers(context, nurMarket.Id, nurDefaultSupplier.Id);
            FixMissingSuppliers(context, demoMarket.Id, demoDefaultSupplier.Id);

            context.SaveChanges();
        }

        // ─── Loglar ───
        if (context.Logs.Count() < 10)
        {
            var adminMarket = context.Markets.FirstOrDefault(m => m.Username == "nur");
            if (adminMarket != null)
            {
                var now = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
                var logs = new List<Log>
                {
                    new Log { MarketId = adminMarket.Id, KullaniciAdi = "nur", IslemTipi = LogType.Ekleme, Detay = "Sistem güncellemeleri yapıldı.", Tarih = now.AddDays(-2) },
                    new Log { MarketId = adminMarket.Id, KullaniciAdi = "nur", IslemTipi = LogType.Login, Detay = "Sisteme ilk yetkili girişi sağlandı.", Tarih = now.AddMinutes(-120) },
                    new Log { MarketId = adminMarket.Id, KullaniciAdi = "nur", IslemTipi = LogType.Ekleme, Detay = "'1kg Tam Buğday Unu' adlı yeni ürün stoğa eklendi (Stok: 50).", Tarih = now.AddMinutes(-115) },
                    new Log { MarketId = adminMarket.Id, KullaniciAdi = "nur", IslemTipi = LogType.Ekleme, Detay = "'1L Günlük Süt' adlı yeni ürün stoğa eklendi (Stok: 120).", Tarih = now.AddMinutes(-110) },
                    new Log { MarketId = adminMarket.Id, KullaniciAdi = "nur", IslemTipi = LogType.Guncelleme, Detay = "'1kg Tam Buğday Unu' ürünü güncellendi (Eski Stok: 50 -> Yeni Stok: 40).", Tarih = now.AddMinutes(-85) },
                    new Log { MarketId = adminMarket.Id, KullaniciAdi = "nur", IslemTipi = LogType.Silme, Detay = "'Hatalı Ürün Kaydı' adlı ürün sistemden tamamen silindi.", Tarih = now.AddMinutes(-30) },
                    new Log { MarketId = adminMarket.Id, KullaniciAdi = "nur", IslemTipi = LogType.Guncelleme, Detay = "Toplu stok güncellemesi yapıldı.", Tarih = now.AddMinutes(-15) },
                    new Log { MarketId = adminMarket.Id, KullaniciAdi = "nur", IslemTipi = LogType.Login, Detay = "Sisteme giriş yapıldı (Oturum Açıldı).", Tarih = now.AddMinutes(-2) }
                };
                context.Logs.AddRange(logs);
                context.SaveChanges();
            }
        }
    }

    private static void GenerateProducts(AppDbContext context, int marketId,
        List<(string Name, string Category)> templates, int count, List<Tedarikci> suppliers, Tedarikci defaultSupplier)
    {
        for (int i = 0; i < count; i++)
        {
            var (name, category) = templates[_rng.Next(templates.Count)];
            var purchasePrice = Math.Round((decimal)(_rng.NextDouble() * 95 + 5), 2);
            var markup = 1.0m + (decimal)(_rng.NextDouble() * 0.3 + 0.1); // %10-%40
            var salePrice = Math.Round(purchasePrice * markup, 2);
            var supplier = GetSupplierForCategory(suppliers, category) ?? defaultSupplier;

            var product = new Product
            {
                Name = name,
                Category = category,
                Barcode = GenerateBarcode(),
                Stock = GetRandomStock(i, count),
                PurchasePrice = purchasePrice,
                SalePrice = salePrice,
                ExpiryDate = DateTime.SpecifyKind(GetRandomExpiryDate(i, count), DateTimeKind.Utc),
                MarketId = marketId,
                TedarikciId = supplier.Id
            };

            context.Products.Add(product);
        }
    }

    private static Tedarikci? GetSupplierForCategory(List<Tedarikci> suppliers, string category)
    {
        return category switch
        {
            "Süt" => suppliers.FirstOrDefault(s => s.Kategori == "Sütçü"),
            "Meyve & Sebze" => suppliers.FirstOrDefault(s => s.Kategori == "Manav"),
            "Dondurulmuş Gıda" => suppliers.FirstOrDefault(s => s.Kategori == "Soğuk Zincir"),
            "Kahvaltılık" => suppliers.FirstOrDefault(s => s.Kategori == "Sütçü"),
            _ => null
        };
    }

    private static void FixMissingSuppliers(AppDbContext context, int marketId, int defaultSupplierId)
    {
        var missing = context.Products
            .Where(p => p.MarketId == marketId && p.TedarikciId == null)
            .ToList();

        foreach (var product in missing)
        {
            product.TedarikciId = defaultSupplierId;
        }
    }

    private static Tedarikci EnsureDefaultSupplier(AppDbContext context, int marketId, List<Tedarikci> suppliers)
    {
        var existing = suppliers.FirstOrDefault(s => s.Ad == "Genel Tedarikçi");
        if (existing != null)
            return existing;

        var defaultSupplier = new Tedarikci
        {
            Ad = "Genel Tedarikçi",
            IletisimNo = "0555 000 00 00",
            TeslimatGunleri = "Her gün",
            Kategori = "Genel",
            MarketId = marketId
        };

        context.Tedarikciler.Add(defaultSupplier);
        context.SaveChanges();
        suppliers.Add(defaultSupplier);

        return defaultSupplier;
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
            // Fırın
            ("Ekmek (200g)", "Fırın"),
            ("Simit (Susamlı)", "Fırın"),
            ("Poğaça (Peynirli)", "Fırın"),
            ("Açma (Tereyağlı)", "Fırın"),

            // Meyve & Sebze
            ("Domates (Salkım)", "Meyve & Sebze"),
            ("Salatalık", "Meyve & Sebze"),
            ("Biber (Çarliston)", "Meyve & Sebze"),
            ("Patates", "Meyve & Sebze"),
            ("Soğan", "Meyve & Sebze"),

            // Süt
            ("Günlük Süt 1L", "Süt"),
            ("1L Laktozsuz Süt", "Süt"),
            ("Sütaş Yoğurt 2kg", "Süt"),
            ("500g Yoğurt", "Süt"),
            ("200g Beyaz Peynir", "Süt"),
            ("200g Kaşar Peynir", "Süt"),
            ("1kg Süzme Yoğurt", "Süt"),
            ("200ml Krema", "Süt"),

            // Kahvaltılık
            ("Yumurta (15'li)", "Kahvaltılık"),
            ("Zeytin (Siyah) 500g", "Kahvaltılık"),
            ("Bal 450g", "Kahvaltılık"),
            ("Reçel 380g", "Kahvaltılık"),

            // İçecek
            ("5L Su", "İçecek"),
            ("1.5L Maden Suyu", "İçecek"),
            ("1L Portakal Suyu", "İçecek"),
            ("500ml Elma Suyu", "İçecek"),
            ("330ml Kola", "İçecek"),
            ("330ml Limonata", "İçecek"),
            ("500ml Ayran", "İçecek"),
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
            ("4'lü Tuvalet Kağıdı", "Temizlik"),

            // Bakliyat
            ("1kg Kırmızı Mercimek", "Bakliyat"),
            ("1kg Pirinç", "Bakliyat"),
            ("1kg Bulgur", "Bakliyat"),
            ("500g Nohut", "Bakliyat"),
            ("500g Kuru Fasulye", "Bakliyat"),
            ("500g Yeşil Mercimek", "Bakliyat"),
            ("1kg Makarna", "Bakliyat"),

            // Çay & Kahve
            ("Çaykur Rize Turist Çay 500g", "Çay & Kahve"),
            ("Türk Kahvesi 100g", "Çay & Kahve"),
            ("Filtre Kahve 250g", "Çay & Kahve"),

            // Dondurulmuş Gıda
            ("1L Dondurma", "Dondurulmuş Gıda"),
            ("400g Dondurulmuş Bezelye", "Dondurulmuş Gıda"),
            ("500g Dondurulmuş Patates", "Dondurulmuş Gıda"),
            ("450g Dondurulmuş Pizza", "Dondurulmuş Gıda"),
            ("300g Dondurulmuş Börek", "Dondurulmuş Gıda"),
            ("250g Dondurulmuş Karides", "Dondurulmuş Gıda"),
            ("500g Dondurulmuş Sebze Karışımı", "Dondurulmuş Gıda")
        };
    }

    private static List<Tedarikci> CreateSuppliers(AppDbContext context, int marketId, List<Tedarikci> templates)
    {
        var suppliers = templates
            .Select(t => new Tedarikci
            {
                Ad = t.Ad,
                IletisimNo = t.IletisimNo,
                TeslimatGunleri = t.TeslimatGunleri,
                Kategori = t.Kategori,
                MarketId = marketId
            })
            .ToList();

        context.Tedarikciler.AddRange(suppliers);
        context.SaveChanges();
        return suppliers;
    }

    private static List<Tedarikci> GetSupplierTemplates()
    {
        return new List<Tedarikci>
        {
            new() { Ad = "Özden Süt Ürünleri", IletisimNo = "0555 111 22 33", TeslimatGunleri = "Pazartesi, Perşembe", Kategori = "Sütçü" },
            new() { Ad = "Kasaba Manav Toptan", IletisimNo = "0555 222 33 44", TeslimatGunleri = "Her gün", Kategori = "Manav" },
            new() { Ad = "Bereket Bakliyat", IletisimNo = "0555 333 44 55", TeslimatGunleri = "Cumartesi", Kategori = "Bakliyatçı" },
            new() { Ad = "Soğuk Zincir Depo", IletisimNo = "0555 444 55 66", TeslimatGunleri = "Salı, Cuma", Kategori = "Soğuk Zincir" },
            new() { Ad = "Genel Tedarikçi", IletisimNo = "0555 000 00 00", TeslimatGunleri = "Her gün", Kategori = "Genel" }
        };
    }
}
