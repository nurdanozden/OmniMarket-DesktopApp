using System.Windows;
using System.Windows.Controls;

namespace OmniMarket.Views;

public partial class LoginView : UserControl
{
    public LoginView()
    {
        InitializeComponent();

        // PasswordBox → ViewModel senkronizasyonu
        PasswordBox.PasswordChanged += (s, e) =>
        {
            if (DataContext is ViewModels.LoginViewModel vm)
                vm.Password = PasswordBox.Password;
        };

        // Test Giriş: PasswordBox'ı otomatik doldur
        Loaded += (s, e) =>
        {
            if (DataContext is ViewModels.LoginViewModel vm)
            {
                vm.PasswordFilled += password =>
                {
                    PasswordBox.Password = password;
                };
            }
        };
    }
}
