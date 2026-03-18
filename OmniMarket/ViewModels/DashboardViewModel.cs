using System.Collections.ObjectModel;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
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

    private PlotModel _expiredChartModel = new();
    private PlotModel _mostSoldChartModel = new();

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

    public PlotModel ExpiredChartModel
    {
        get => _expiredChartModel;
        set => SetProperty(ref _expiredChartModel, value);
    }

    public PlotModel MostSoldChartModel
    {
        get => _mostSoldChartModel;
        set => SetProperty(ref _mostSoldChartModel, value);
    }

    public RelayCommand RefreshCommand { get; }

    public DashboardViewModel()
    {
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

        // Charts
        LoadExpiredChart();
        LoadMostSoldChart();
    }

    private void LoadExpiredChart()
    {
        var model = new PlotModel
        {
            Background = OxyColors.Transparent,
            PlotAreaBorderColor = OxyColors.Transparent,
            TextColor = OxyColor.Parse("#f8fafc")
        };

        var products = _productService.GetProducts(_marketId);
        var expiredTop5 = products
            .Where(p => DateTime.Today > p.ExpiryDate)
            .GroupBy(p => p.Name)
            .Select(g => new { Name = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToList();

        var categoryAxis = new CategoryAxis
        {
            Position = AxisPosition.Left,
            Key = "ProductAxis",
            TextColor = OxyColor.Parse("#94a3b8"),
            AxislineColor = OxyColor.Parse("#334155"),
            TicklineColor = OxyColor.Parse("#334155")
        };

        foreach (var item in expiredTop5)
        {
            categoryAxis.Labels.Add(item.Name.Length > 12 ? item.Name[..12] + "…" : item.Name);
        }
        model.Axes.Add(categoryAxis);

        var valueAxis = new LinearAxis
        {
            Position = AxisPosition.Bottom,
            MinimumPadding = 0,
            AbsoluteMinimum = 0,
            TextColor = OxyColor.Parse("#94a3b8"),
            AxislineColor = OxyColor.Parse("#334155"),
            MajorGridlineStyle = LineStyle.Solid,
            MajorGridlineColor = OxyColor.Parse("#1e293b")
        };
        model.Axes.Add(valueAxis);

        var barSeries = new BarSeries
        {
            FillColor = OxyColor.Parse("#ef4444"),
            StrokeColor = OxyColors.Transparent,
            LabelPlacement = LabelPlacement.Inside,
            LabelFormatString = "{0}"
        };

        foreach (var item in expiredTop5)
        {
            barSeries.Items.Add(new BarItem { Value = item.Count });
        }

        model.Series.Add(barSeries);
        ExpiredChartModel = model;
    }

    private void LoadMostSoldChart()
    {
        var model = new PlotModel
        {
            Background = OxyColors.Transparent,
            PlotAreaBorderColor = OxyColors.Transparent,
            TextColor = OxyColor.Parse("#f8fafc")
        };

        var products = _productService.GetProducts(_marketId);
        var leastStock = products
            .OrderBy(p => p.Stock)
            .Take(5)
            .ToList();

        var pieSeries = new PieSeries
        {
            StrokeThickness = 2,
            InsideLabelPosition = 0.8,
            AngleSpan = 360,
            StartAngle = 0
        };

        var colors = new[]
        {
            "#22c55e", "#16a34a", "#15803d", "#4ade80", "#86efac"
        };

        for (int i = 0; i < leastStock.Count; i++)
        {
            var p = leastStock[i];
            pieSeries.Slices.Add(new PieSlice(p.Name.Length > 15 ? p.Name[..15] + "…" : p.Name, Math.Max(1, 100 - p.Stock))
            {
                Fill = OxyColor.Parse(colors[i % colors.Length])
            });
        }

        model.Series.Add(pieSeries);
        MostSoldChartModel = model;
    }
}

public class AlertItem
{
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}
