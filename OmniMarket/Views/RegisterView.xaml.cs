using System.Windows;
using System.Windows.Controls;

namespace OmniMarket.Views;

public partial class RegisterView : UserControl
{
    public RegisterView()
    {
        InitializeComponent();
        
        PasswordBox.PasswordChanged += (s, e) =>
        {
            if (DataContext is ViewModels.RegisterViewModel vm)
                vm.Password = PasswordBox.Password;
                
            if (PasswordPlaceholder != null)
                PasswordPlaceholder.Visibility = string.IsNullOrEmpty(PasswordBox.Password) 
                    ? Visibility.Visible : Visibility.Collapsed;
        };

        Loaded += (s, e) =>
        {
            if (PasswordPlaceholder != null)
                PasswordPlaceholder.Visibility = string.IsNullOrEmpty(PasswordBox.Password) 
                    ? Visibility.Visible : Visibility.Collapsed;
        };
    }
}
