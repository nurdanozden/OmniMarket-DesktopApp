using System;
using System.Collections.ObjectModel;
using System.Linq;
using OmniMarket.Data;
using OmniMarket.Helpers;
using OmniMarket.Models;

namespace OmniMarket.ViewModels;

public class SupplierDetailViewModel : BaseViewModel
{
    private Tedarikci _supplier = null!;
    private ObservableCollection<ProductRowItem> _products = new();

    // ── Tedarikçi Bilgileri ─────────────────────────────────────────────
    public string SupplierName      => _supplier?.Ad            ?? string.Empty;
    public string SupplierCategory  => _supplier?.Kategori      ?? string.Empty;
    public string SupplierPhone     => _supplier?.IletisimNo    ?? string.Empty;
    public string SupplierDays      => _supplier?.TeslimatGunleri ?? string.Empty;

    // Mock contact info (Tedarikci modeline adres/yetkili eklenmeden önce placeholder)
    public string SupplierAddress        => "Kuzey Sanayi Bölgesi, No:14, İstanbul";
    public string SupplierAuthorizedName => "Ahmet Yılmaz";
    public string ActiveOrderCount       => "3 Aktif Sipariş";

    // ── Ürün Portföyü ──────────────────────────────────────────────────
    public ObservableCollection<ProductRowItem> Products
    {
        get => _products;
        set => SetProperty(ref _products, value);
    }

    // ── Finansal Özet ──────────────────────────────────────────────────
    private decimal _totalCost;
    private decimal _paidAmount;
    private decimal _remainingDebt;

    public decimal TotalCost
    {
        get => _totalCost;
        set => SetProperty(ref _totalCost, value);
    }
    public decimal PaidAmount
    {
        get => _paidAmount;
        set => SetProperty(ref _paidAmount, value);
    }
    public decimal RemainingDebt
    {
        get => _remainingDebt;
        set => SetProperty(ref _remainingDebt, value);
    }

    // ── Performans Metriği ─────────────────────────────────────────────
    private int    _totalProductCount;
    private int    _expiredProductCount;
    private double _wasteRate;

    public int TotalProductCount
    {
        get => _totalProductCount;
        set => SetProperty(ref _totalProductCount, value);
    }
    public int ExpiredProductCount
    {
        get => _expiredProductCount;
        set => SetProperty(ref _expiredProductCount, value);
    }
    public double WasteRate
    {
        get => _wasteRate;
        set => SetProperty(ref _wasteRate, value);
    }
    public string WasteRateText => $"%{WasteRate:N1}";

    // ── Güvenilirlik Skoru ───────────────────────────────────────────
    private double _reliabilityScore;
    public double ReliabilityScore
    {
        get => _reliabilityScore;
        set => SetProperty(ref _reliabilityScore, value);
    }
    public string ReliabilityLabel => ReliabilityScore >= 85 ? "Mükemmel"
                                    : ReliabilityScore >= 65 ? "İyi"
                                    : ReliabilityScore >= 40 ? "Orta"
                                    : "Kritik";
    public string ReliabilityColor => ReliabilityScore >= 85 ? "#059669"
                                    : ReliabilityScore >= 65 ? "#D97706"
                                    : ReliabilityScore >= 40 ? "#EA580C"
                                    : "#DC2626";

    // ── Commands ─────────────────────────────────────────────────────────
    public RelayCommand GoBackCommand { get; }
    public event Action? BackRequested;

    public SupplierDetailViewModel()
    {
        GoBackCommand = new RelayCommand(() => BackRequested?.Invoke());
    }

    public void Initialize(Tedarikci supplier)
    {
        _supplier = supplier;

        OnPropertyChanged(nameof(SupplierName));
        OnPropertyChanged(nameof(SupplierCategory));
        OnPropertyChanged(nameof(SupplierPhone));
        OnPropertyChanged(nameof(SupplierDays));

        LoadData();
    }

    private void LoadData()
    {
        using var db = new AppDbContext();

        // JOIN: Bu tedarikçiye ait ürünleri getir
        var rawProducts = db.Products
            .Where(p => p.TedarikciId == _supplier.Id)
            .OrderBy(p => p.ExpiryDate)
            .ToList();

        // Ürünleri isme göre grupla (aynı ürün birden fazla kayıtla olabilir)
        var grouped = rawProducts
            .GroupBy(p => p.Name)
            .Select(g =>
            {
                var first     = g.OrderBy(p => p.ExpiryDate).First();
                int totalStock = g.Sum(p => p.Stock);
                decimal totalVal = g.Sum(p => p.PurchasePrice * p.Stock);
                bool hasExpired  = first.ExpiryDate < DateTime.Today;
                bool isExpiring  = !hasExpired && (first.ExpiryDate - DateTime.Today).TotalDays <= 7;

                return new ProductRowItem
                {
                    Name          = first.Name,
                    Category      = first.Category,
                    Stock         = totalStock,
                    PurchasePrice = first.PurchasePrice,
                    TotalValue    = totalVal,
                    ExpiryDate    = first.ExpiryDate,
                    StatusText    = hasExpired ? "SKT Geçmiş" : (isExpiring ? "SKT Yaklaşıyor" : "Normal"),
                    StatusColor   = hasExpired ? "#DC2626"    : (isExpiring ? "#D97706"       : "#059669")
                };
            })
            .ToList();

        Products = new ObservableCollection<ProductRowItem>(grouped);

        // ── Finansal Özet ───────────────────────────────────────────────
        TotalCost     = rawProducts.Sum(p => p.PurchasePrice * p.Stock);
        PaidAmount    = Math.Round(TotalCost * 0.70m, 2);   // Mock: %70 ödenmiş
        RemainingDebt = TotalCost - PaidAmount;

        // ── Performans ──────────────────────────────────────────────────
        TotalProductCount   = grouped.Count;
        ExpiredProductCount = grouped.Count(r => r.StatusText == "SKT Geçmiş");
        WasteRate           = TotalProductCount > 0
            ? Math.Round((double)ExpiredProductCount / TotalProductCount * 100, 1)
            : 0;

        OnPropertyChanged(nameof(WasteRateText));

        // Güvenilirlik Skoru: zayiat oranının tersi (100 - wasteRate)
        ReliabilityScore = Math.Max(0, Math.Min(100, 100 - WasteRate));
        OnPropertyChanged(nameof(ReliabilityLabel));
        OnPropertyChanged(nameof(ReliabilityColor));
    }
}

public class ProductRowItem
{
    public string  Name          { get; set; } = string.Empty;
    public string  Category      { get; set; } = string.Empty;
    public int     Stock         { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal TotalValue    { get; set; }
    public DateTime ExpiryDate   { get; set; }
    public string  StatusText    { get; set; } = string.Empty;
    public string  StatusColor   { get; set; } = string.Empty;
    public string  ExpiryDateText => ExpiryDate.ToString("dd.MM.yyyy");
}
