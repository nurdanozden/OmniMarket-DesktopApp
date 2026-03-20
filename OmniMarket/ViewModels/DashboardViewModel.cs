using System.Collections.ObjectModel;
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
        // Stats
        var stats = _productService.GetDashboardStats(_marketId);
        TotalProducts = stats.TotalProducts;
        ExpiringProducts = stats.ExpiringProducts;
        ExpiredProducts = stats.ExpiredProducts;
        TotalStockValue = stats.TotalStockValue;

        // Alerts
        var alertList = _productService.GetAlerts(_marketId);
        Alerts = new ObservableCollection<AlertItem>(
            alertList.Select(a => new AlertItem { Message = a.Message, Type = a.Type }));

        // Top Lists (OxyPlot grafikleri yerine kullanılıyor)
        LoadTopExpiredProducts();
        LoadTopSoldProducts();
    }

    private void LoadTopExpiredProducts()
    {
        var products = _productService.GetProducts(_marketId);
        var expiredTop3 = products
            .Where(p => DateTime.Today > p.ExpiryDate)
            .GroupBy(p => p.Name)
            .Select(g => new { Name = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(3)
            .ToList();

        var list = new ObservableCollection<TopProductItem>();
        int rank = 1;

        // Turuncu - Mercan - Kırmızı tonları
        string[] colors = { "#ef4444", "#f97316", "#f59e0b" };

        foreach (var item in expiredTop3)
        {
            list.Add(new TopProductItem
            {
                Rank = rank,
                Name = item.Name,
                ValueText = $"{item.Count} Adet",
                Icon = "🔥",
                ColorHex = colors[(rank - 1) % colors.Length]
            });
            rank++;
        }

        TopExpiredProducts = list;
    }

    private void LoadTopSoldProducts()
    {
        var products = _productService.GetProducts(_marketId);
        var leastStockTop3 = products
            .OrderBy(p => p.Stock)
            .Take(3)
            .ToList();

        var list = new ObservableCollection<TopProductItem>();
        int rank = 1;

        // Mint - Turkuaz - Mavi tonları
        string[] colors = { "#2DD4BF", "#06b6d4", "#3b82f6" };

        foreach (var p in leastStockTop3)
        {
            // En çok satılanlar için varsayımsal bir satış hızı/tahmin değeri formülü
            var salesValue = Math.Max(1, 100 - p.Stock); 
            list.Add(new TopProductItem
            {
                Rank = rank,
                Name = p.Name,
                ValueText = $"{salesValue} Satış",
                Icon = "📈",
                ColorHex = colors[(rank - 1) % colors.Length]
            });
            rank++;
        }

        TopSoldProducts = list;
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
