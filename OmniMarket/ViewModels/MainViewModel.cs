using OmniMarket.Helpers;
using OmniMarket.Models;

namespace OmniMarket.ViewModels;

public class MainViewModel : BaseViewModel
{
    private object _currentView = null!;
    private Market _currentMarket = null!;
    private string _currentViewName = "Dashboard";

    public object CurrentView
    {
        get => _currentView;
        set => SetProperty(ref _currentView, value);
    }

    public Market CurrentMarket
    {
        get => _currentMarket;
        set => SetProperty(ref _currentMarket, value);
    }

    public string CurrentViewName
    {
        get => _currentViewName;
        set => SetProperty(ref _currentViewName, value);
    }

    public bool IsAdmin => CurrentMarket != null && (CurrentMarket.Username == "nur" || CurrentMarket.Username == "admin");

    public RelayCommand ShowDashboardCommand { get; }
    public RelayCommand ShowProductsCommand { get; }
    public RelayCommand ShowLogsCommand { get; }
    public RelayCommand LogoutCommand { get; }

    public event Action? LoggedOut;

    public MainViewModel()
    {
        ShowDashboardCommand = new RelayCommand(NavigateToDashboard);
        ShowProductsCommand = new RelayCommand(NavigateToProducts);
        ShowLogsCommand = new RelayCommand(NavigateToLogs);
        LogoutCommand = new RelayCommand(ExecuteLogout);
    }

    public void Initialize(Market market)
    {
        CurrentMarket = market;
        OnPropertyChanged(nameof(IsAdmin));
        NavigateToDashboard();
    }

    private void NavigateToDashboard()
    {
        var vm = new DashboardViewModel();
        vm.NavigateRequested += OnDashboardNavigateRequested;
        vm.Initialize(CurrentMarket.Id);
        CurrentView = vm;
        CurrentViewName = "Dashboard";
    }

    private void NavigateToProducts()
    {
        NavigateToProductsFiltered(null);
    }

    private void NavigateToProductsFiltered(string? filter)
    {
        var vm = new ProductListViewModel();
        vm.Initialize(CurrentMarket.Id, CurrentMarket.Username, filter);
        CurrentView = vm;
        CurrentViewName = "Ürünler";
    }

    private void NavigateToLogs()
    {
        var vm = new LogListViewModel();
        vm.Initialize(CurrentMarket.Id);
        CurrentView = vm;
        CurrentViewName = "İşlem Geçmişi";
    }

    /// <summary>
    /// DashboardViewModel'deki NavigateRequested event handler'ı.
    /// </summary>
    private void OnDashboardNavigateRequested(string filter)
    {
        NavigateToProductsFiltered(filter);
    }

    private void ExecuteLogout()
    {
        LoggedOut?.Invoke();
    }
}
