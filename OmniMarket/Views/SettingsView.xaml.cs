using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using OmniMarket.ViewModels;

namespace OmniMarket.Views;

public partial class SettingsView : UserControl
{
    private static readonly SolidColorBrush ActiveFg   = new(Color.FromRgb(16, 185, 129));   // Emerald #10B981
    private static readonly SolidColorBrush InactiveFg = new(Color.FromRgb(100, 116, 139));  // Slate-500 #64748B
    private static readonly SolidColorBrush ActiveBg   = new(Color.FromRgb(236, 253, 245));  // Emerald-50 #ECFDF5
    private static readonly SolidColorBrush TransparentBg = new(Colors.Transparent);

    public SettingsView()
    {
        InitializeComponent();
        // İlk tab aktif olarak başlasın
        Loaded += (_, _) => ActivateTab(TabGeneral, "General");
    }

    private void Tab_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn) return;
        ActivateTab(btn, btn.Tag?.ToString());
    }

    private void ActivateTab(Button activeBtn, string? tag)
    {
        // Panel görünürlükleri
        PanelGeneral.Visibility       = tag == "General"       ? Visibility.Visible : Visibility.Collapsed;
        PanelProfile.Visibility       = tag == "Profile"       ? Visibility.Visible : Visibility.Collapsed;
        PanelSystem.Visibility        = tag == "System"        ? Visibility.Visible : Visibility.Collapsed;

        // Tüm tab butonlarını pasife al
        foreach (var btn in new[] { TabGeneral, TabProfile, TabSystem })
        {
            btn.Foreground   = InactiveFg;
            btn.Background   = TransparentBg;
            btn.BorderBrush  = TransparentBg;
            btn.BorderThickness = new Thickness(0, 0, 0, 2);
        }

        // Aktif tab'ı vurgula
        activeBtn.Foreground     = ActiveFg;
        activeBtn.Background     = ActiveBg;
        activeBtn.BorderBrush    = ActiveFg;
        activeBtn.BorderThickness = new Thickness(0, 0, 0, 2);
    }

    private void OldPasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsViewModel vm && sender is PasswordBox box)
            vm.OldPassword = box.Password;
    }

    private void NewPasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsViewModel vm && sender is PasswordBox box)
            vm.NewPassword = box.Password;
    }

    private void ConfirmPasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsViewModel vm && sender is PasswordBox box)
            vm.ConfirmNewPassword = box.Password;
    }
}

