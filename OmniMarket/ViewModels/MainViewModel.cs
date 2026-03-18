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

    public RelayCommand ShowDashboardCommand { get; }
    public RelayCommand ShowProductsCommand { get; }
    public RelayCommand LogoutCommand { get; }

    public event Action? LoggedOut;

    public MainViewModel()
    {
        ShowDashboardCommand = new RelayCommand(NavigateToDashboard);
        ShowProductsCommand = new RelayCommand(NavigateToProducts);
        LogoutCommand = new RelayCommand(ExecuteLogout);
    }

    public void Initialize(Market market)
    {
        CurrentMarket = market;
        NavigateToDashboard();
    }

    private void NavigateToDashboard()
    {
        var vm = new DashboardViewModel();
        vm.Initialize(CurrentMarket.Id);
        CurrentView = vm;
        CurrentViewName = "Dashboard";
    }

    private void NavigateToProducts()
    {
        var vm = new ProductListViewModel();
        vm.Initialize(CurrentMarket.Id);
        CurrentView = vm;
        CurrentViewName = "Ürünler";
    }

    private void ExecuteLogout()
    {
        LoggedOut?.Invoke();
    }
}
