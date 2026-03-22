using System.Collections.ObjectModel;
using System.Windows.Data;
using OmniMarket.Helpers;
using OmniMarket.Models;
using OmniMarket.Services;

namespace OmniMarket.ViewModels;

public class LogListViewModel : BaseViewModel
{
    private readonly LogService _logService;
    private int _marketId;
    private string _searchText = string.Empty;
    
    // Asıl veri listesi (Full List)
    private ObservableCollection<Log> _allLogs = new();
    
    // UI'da gösterilen (Filtrelenmiş) Koleksiyon
    public CollectionViewSource FilteredLogsView { get; } = new CollectionViewSource();

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                FilteredLogsView.View.Refresh();
            }
        }
    }

    public ObservableCollection<Log> AllLogs
    {
        get => _allLogs;
        set => SetProperty(ref _allLogs, value);
    }

    public LogListViewModel()
    {
        _logService = new LogService();
        FilteredLogsView.Source = _allLogs;
        FilteredLogsView.View.Filter = FilterLogs;
    }

    public async void Initialize(int marketId)
    {
        _marketId = marketId;
        await LoadLogsAsync();
    }

    private async Task LoadLogsAsync()
    {
        var logs = await _logService.GetLogsAsync(_marketId);
        AllLogs.Clear();
        foreach (var log in logs)
        {
            AllLogs.Add(log);
        }
        FilteredLogsView.View.Refresh();
    }

    private bool FilterLogs(object obj)
    {
        if (obj is not Log log) return false;
        if (string.IsNullOrWhiteSpace(SearchText)) return true;

        var searchLower = SearchText.ToLower();

        return log.KullaniciAdi.ToLower().Contains(searchLower) ||
               log.Detay.ToLower().Contains(searchLower) ||
               log.IslemTipi.ToString().ToLower().Contains(searchLower);
    }
}
