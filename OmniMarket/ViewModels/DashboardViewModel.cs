using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
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

    // Navigation Commands
    public RelayCommand NavigateToAllProductsCommand  { get; }
    public RelayCommand NavigateToExpiringCommand     { get; }
    public RelayCommand NavigateToExpiredCommand      { get; }
    public RelayCommand NavigateToStockValueCommand   { get; }
    public RelayCommand RefreshCommand                { get; }

    // Action Commands
    public RelayCommand ApplyCampaignCommand { get; }
    public RelayCommand CreateReturnCommand  { get; }

    public event Action<string>? NavigateRequested;

    public DashboardViewModel()
    {
        NavigateToAllProductsCommand = new RelayCommand(() => NavigateRequested?.Invoke("all"));
        NavigateToExpiringCommand    = new RelayCommand(() => NavigateRequested?.Invoke("expiring"));
        NavigateToExpiredCommand     = new RelayCommand(() => NavigateRequested?.Invoke("expired"));
        NavigateToStockValueCommand  = new RelayCommand(() => NavigateRequested?.Invoke("stockvalue"));
        RefreshCommand               = new RelayCommand(LoadData);

        ApplyCampaignCommand = new RelayCommand(ExecuteApplyCampaign);
        CreateReturnCommand  = new RelayCommand(ExecuteCreateReturn);
    }

    public void Initialize(int marketId)
    {
        _marketId = marketId;
        LoadData();
    }

    private void LoadData()
    {
        // Gerçek veritabanı istatistikleri
        var (total, expiring, expired, stockValue) = _productService.GetDashboardStats(_marketId);
        TotalProducts    = total;
        ExpiringProducts = expiring;
        ExpiredProducts  = expired;
        TotalStockValue  = stockValue;

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

    private void ExecuteApplyCampaign()
    {
        if (ExpiringProducts == 0)
        {
            MessageBox.Show("SKT yaklaşan ürün bulunmuyor.", "Kampanya",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        // Basit input dialog
        var dialog = new System.Windows.Window
        {
            Title = "Hızlı Kampanya Tanımla",
            Width = 360, Height = 175,
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen,
            ResizeMode = System.Windows.ResizeMode.NoResize,
            WindowStyle = System.Windows.WindowStyle.ToolWindow,
            Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(248, 250, 252))
        };

        var panel = new System.Windows.Controls.StackPanel { Margin = new System.Windows.Thickness(20) };
        panel.Children.Add(new System.Windows.Controls.TextBlock
        {
            Text = $"SKT yaklaşan {ExpiringProducts} ürün için indirim oranı girin (%):",
            TextWrapping = System.Windows.TextWrapping.Wrap,
            Margin = new System.Windows.Thickness(0, 0, 0, 10),
            FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
            Foreground = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(15, 23, 42))
        });

        var textBox = new System.Windows.Controls.TextBox
        {
            Text = "10",
            Padding = new System.Windows.Thickness(10, 8, 10, 8),
            FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
            FontSize = 14,
            Margin = new System.Windows.Thickness(0, 0, 0, 12)
        };
        panel.Children.Add(textBox);

        var btnPanel = new System.Windows.Controls.StackPanel
        {
            Orientation = System.Windows.Controls.Orientation.Horizontal,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Right
        };
        var okBtn = new System.Windows.Controls.Button
        {
            Content = "Uygula",
            Padding = new System.Windows.Thickness(16, 8, 16, 8),
            Margin = new System.Windows.Thickness(0, 0, 8, 0),
            IsDefault = true
        };
        var cancelBtn = new System.Windows.Controls.Button
        {
            Content = "İptal",
            Padding = new System.Windows.Thickness(16, 8, 16, 8),
            IsCancel = true
        };

        bool confirmed = false;
        okBtn.Click += (_, _) => { confirmed = true; dialog.Close(); };
        cancelBtn.Click += (_, _) => dialog.Close();

        btnPanel.Children.Add(okBtn);
        btnPanel.Children.Add(cancelBtn);
        panel.Children.Add(btnPanel);
        dialog.Content = panel;
        dialog.ShowDialog();

        if (!confirmed) return;

        if (!decimal.TryParse(textBox.Text.Replace(",", "."),
            System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture,
            out decimal rate) || rate <= 0 || rate > 100)
        {
            MessageBox.Show("Geçerli bir oran girin (1–100).", "Hata",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        int updated = _productService.ApplyCampaign(_marketId, rate);
        MessageBox.Show($"✅ {updated} ürüne %{rate:N0} kampanya indirimi uygulandı.",
            "Kampanya Oluşturuldu", MessageBoxButton.OK, MessageBoxImage.Information);
        LoadData();
    }

    private void ExecuteCreateReturn()
    {
        if (ExpiringProducts == 0)
        {
            MessageBox.Show("SKT yaklaşan ürün bulunmuyor.", "İade Talebi",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var result = MessageBox.Show(
            $"SKT'si yaklaşan {ExpiringProducts} ürün tedarikçiye iade listesine eklensin mi?",
            "İade Talebi Oluştur",
            MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        int updated = _productService.RequestReturn(_marketId);
        MessageBox.Show($"✅ {updated} ürün için iade talebi oluşturuldu.",
            "İade Talebi", MessageBoxButton.OK, MessageBoxImage.Information);
        LoadData();
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
                Rank      = index + 1,
                Name      = p.Name,
                ValueText = $"{p.Stock} Adet",
                Icon      = "⚠",
                ColorHex  = "#DC2626"
            })
            .ToList();

        TopExpiredProducts = new ObservableCollection<TopProductItem>(expired);
    }

    private void LoadTopSoldProducts()
    {
        TopSoldProducts = new ObservableCollection<TopProductItem>
        {
            new() { Rank = 1, Name = "Ekmek (200g)",    ValueText = "450 Satış", Icon = "↑", ColorHex = "#1D4ED8" },
            new() { Rank = 2, Name = "Yumurta (15'li)", ValueText = "120 Satış", Icon = "↑", ColorHex = "#059669" },
            new() { Rank = 3, Name = "5L Su",           ValueText = "85 Satış",  Icon = "↑", ColorHex = "#1D4ED8" }
        };
    }
}

public class AlertItem
{
    public string Message { get; set; } = string.Empty;
    public string Type    { get; set; } = string.Empty;
}

public class TopProductItem
{
    public int    Rank      { get; set; }
    public string Name      { get; set; } = string.Empty;
    public string ValueText { get; set; } = string.Empty;
    public string Icon      { get; set; } = string.Empty;
    public string ColorHex  { get; set; } = string.Empty;
}
