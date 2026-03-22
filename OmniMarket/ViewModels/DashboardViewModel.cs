using System;
using System.Collections.ObjectModel;
using System.Linq;
using OmniMarket.Helpers;
using OmniMarket.Models;
using OmniMarket.Services;

namespace OmniMarket.ViewModels;

public class DashboardViewModel : BaseViewModel
{
    private readonly ProductService _productService = new();
    private int _marketId;

    private int _totalProducts;
    private int _expiringProducts;
    private int _expiredProducts;
    private decimal _totalStockValue;
    private ObservableCollection<AlertItem> _alerts = new();

    private ObservableCollection<TopProductItem> _topExpiredProducts = new();
    private ObservableCollection<TopProductItem> _topSoldProducts = new();

    public int TotalProducts
    {
        get => _totalProducts;
        set => SetProperty(ref _totalProducts, value);
    }

    public int ExpiringProducts
    {
        get => _expiringProducts;
        set => SetProperty(ref _expiringProducts, value);
    }

    public int ExpiredProducts
    {
        get => _expiredProducts;
        set => SetProperty(ref _expiredProducts, value);
    }

    public decimal TotalStockValue
    {
        get => _totalStockValue;
        set => SetProperty(ref _totalStockValue, value);
    }

    public ObservableCollection<AlertItem> Alerts
    {
        get => _alerts;
        set => SetProperty(ref _alerts, value);
    }

    public ObservableCollection<TopProductItem> TopExpiredProducts
    {
        get => _topExpiredProducts;
        set => SetProperty(ref _topExpiredProducts, value);
    }

    public ObservableCollection<TopProductItem> TopSoldProducts
    {
        get => _topSoldProducts;
        set => SetProperty(ref _topSoldProducts, value);
    }

    // ─── Navigation Commands (Kart tıklamaları) ─────────────────────────
    public RelayCommand NavigateToAllProductsCommand { get; }
    public RelayCommand NavigateToExpiringCommand { get; }
    public RelayCommand NavigateToExpiredCommand { get; }
    public RelayCommand NavigateToStockValueCommand { get; }
    public RelayCommand RefreshCommand { get; }

    /// <summary>
    /// MainViewModel bu event'e subscribe olur — parametre filtre tipidir.
    /// </summary>
    public event Action<string>? NavigateRequested;

    public DashboardViewModel()
    {
        NavigateToAllProductsCommand = new RelayCommand(() => NavigateRequested?.Invoke("all"));
        NavigateToExpiringCommand = new RelayCommand(() => NavigateRequested?.Invoke("expiring"));
        NavigateToExpiredCommand = new RelayCommand(() => NavigateRequested?.Invoke("expired"));
        NavigateToStockValueCommand = new RelayCommand(() => NavigateRequested?.Invoke("stockvalue"));
        RefreshCommand = new RelayCommand(LoadData);
    }

    public void Initialize(int marketId)
    {
        _marketId = marketId;
        LoadData();
    }

    private void LoadData()
    {
        // Mock stats for realistic neighborhood market data
        TotalProducts = 356;
        ExpiringProducts = 24;
        ExpiredProducts = 4;
        TotalStockValue = 145250.00m;

        // Alerts
        Alerts = new ObservableCollection<AlertItem>
        {
            new() { Message = "DİKKAT: 'Sütaş Yoğurt 2kg' ürününün 5 adedinin SKT'sine sadece 2 gün kaldı! Kampanya başlatılabilir.", Type = "Warning" },
            new() { Message = "STOK KRİTİK: 'Çaykur Rize Turist Çay 500g' stokta sadece 2 adet kaldı. Tedarikçiye sipariş geçilmeli.", Type = "LowStock" },
            new() { Message = "BİLGİ: Haftalık satış analizine göre 'Dondurma' satışları %20 arttı, stokları kontrol et.", Type = "Info" }
        };

        LoadTopExpiredProducts();
        LoadTopSoldProducts();
    }

    private void LoadTopExpiredProducts()
    {
        var expired = _productService
            .GetProducts(_marketId)
            .Where(p => p.ExpiryDate.Date < DateTime.Today)
            .OrderByDescending(p => p.Stock)
            .Take(3)
            .Select((p, index) => new TopProductItem
            {
                Rank = index + 1,
                Name = p.Name,
                ValueText = $"{p.Stock} Adet",
                Icon = "🔥",
                ColorHex = "#F472B6"
            })
            .ToList();

        TopExpiredProducts = new ObservableCollection<TopProductItem>(expired);
    }

    private void LoadTopSoldProducts()
    {
        TopSoldProducts = new ObservableCollection<TopProductItem>
        {
            new() { Rank = 1, Name = "Ekmek (200g)", ValueText = "450 Satış", Icon = "🍞", ColorHex = "#3B82F6" },
            new() { Rank = 2, Name = "Yumurta (15'li)", ValueText = "120 Satış", Icon = "🥚", ColorHex = "#2DD4BF" },
            new() { Rank = 3, Name = "5L Su", ValueText = "85 Satış", Icon = "💧", ColorHex = "#60A5FA" }
        };
    }
}

public class AlertItem
{
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}

public class TopProductItem
{
    public int Rank { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ValueText { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string ColorHex { get; set; } = string.Empty;
}
