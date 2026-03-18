using System.Windows;
using OmniMarket.ViewModels;

namespace OmniMarket;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}