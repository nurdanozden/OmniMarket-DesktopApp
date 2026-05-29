using System.Windows;
using System.Windows.Controls;

namespace OmniMarket.Views;

public partial class LoginView : UserControl
{
    public LoginView()
    {
        InitializeComponent();

        PasswordBox.PasswordChanged += (s, e) =>
        {
            if (DataContext is ViewModels.LoginViewModel vm)
                vm.Password = PasswordBox.Password;
                
            PasswordPlaceholder.Visibility = string.IsNullOrEmpty(PasswordBox.Password) 
                ? Visibility.Visible : Visibility.Collapsed;
        };

        Loaded += (s, e) =>
        {
            if (DataContext is ViewModels.LoginViewModel vm)
            {
                vm.PasswordFilled += password =>
                {
                    PasswordBox.Password = password;
                };
            }
            
            PasswordPlaceholder.Visibility = string.IsNullOrEmpty(PasswordBox.Password) 
                ? Visibility.Visible : Visibility.Collapsed;
        };
    }
}

