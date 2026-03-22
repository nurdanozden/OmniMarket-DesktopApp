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
        };
    }
}
