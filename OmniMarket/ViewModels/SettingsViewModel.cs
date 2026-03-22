using System.Windows;
using System.Windows.Media;
using OmniMarket.Helpers;
using OmniMarket.Data;
using OmniMarket.Models;

namespace OmniMarket.ViewModels;

public class SettingsViewModel : BaseViewModel
{
    private int _marketId;
    private string _marketAdi = string.Empty;
    private string _adres = string.Empty;
    private string _logoPath = string.Empty;
    private string _temaRengi = "#3B82F6";
    private string _oldPassword = string.Empty;
    private string _newPassword = string.Empty;
    private string _confirmNewPassword = string.Empty;

    public string MarketAdi { get => _marketAdi; set => SetProperty(ref _marketAdi, value); }
    public string Adres { get => _adres; set => SetProperty(ref _adres, value); }
    public string LogoPath { get => _logoPath; set => SetProperty(ref _logoPath, value); }
    public string TemaRengi { get => _temaRengi; set => SetProperty(ref _temaRengi, value); }
    
    public string OldPassword { get => _oldPassword; set => SetProperty(ref _oldPassword, value); }
    public string NewPassword { get => _newPassword; set => SetProperty(ref _newPassword, value); }
    public string ConfirmNewPassword { get => _confirmNewPassword; set => SetProperty(ref _confirmNewPassword, value); }

    public RelayCommand SaveCommand { get; }
    public RelayCommand ApplyThemeCommand { get; }

    public event Action<string>? MarketNameUpdated;

    public SettingsViewModel()
    {
        SaveCommand = new RelayCommand(ExecuteSave);
        ApplyThemeCommand = new RelayCommand(ExecuteApplyTheme);
    }

    public void Initialize(int marketId)
    {
        _marketId = marketId;
        using var db = new AppDbContext();
        var market = db.Markets.FirstOrDefault(m => m.Id == marketId);
        var profile = db.MarketProfiller.FirstOrDefault(p => p.MarketId == marketId);

        if (profile == null)
        {
            profile = new MarketProfil
            {
                MarketId = marketId,
                MarketAdi = market?.Name ?? "NUR MARKET",
                TemaRengi = "#3B82F6"
            };
            db.MarketProfiller.Add(profile);
            db.SaveChanges();
        }

        MarketAdi = profile.MarketAdi;
        Adres = profile.Adres;
        LogoPath = profile.LogoPath;
        TemaRengi = profile.TemaRengi;
        ApplyThemeColor(TemaRengi);
    }

    private void ExecuteSave()
    {
        using var db = new AppDbContext();
        var market = db.Markets.FirstOrDefault(m => m.Id == _marketId);
        var profile = db.MarketProfiller.FirstOrDefault(p => p.MarketId == _marketId);
        if (market == null || profile == null)
        {
            MessageBox.Show("Market profili bulunamad�.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        // Password change logic
        if (!string.IsNullOrEmpty(OldPassword) || !string.IsNullOrEmpty(NewPassword) || !string.IsNullOrEmpty(ConfirmNewPassword))
        {
            if (market.Password != OldPassword)
            {
                MessageBox.Show("Hatalı eski şifre", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (NewPassword != ConfirmNewPassword)
            {
                MessageBox.Show("Yeni şifreler eşleşmiyor!", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(NewPassword))
            {
                MessageBox.Show("Yeni şifre boş olamaz!", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            market.Password = NewPassword;
        }

        market.Name = MarketAdi;

        profile.MarketAdi = MarketAdi;
        profile.Adres = Adres;
        profile.LogoPath = LogoPath;
        profile.TemaRengi = TemaRengi;

        db.SaveChanges();
        
        // Reset password fields internally, NOTE: UI PasswordBoxes won't clear magically unless we pass events or use special bindings, but this clears VM state.
        OldPassword = string.Empty;
        NewPassword = string.Empty;
        ConfirmNewPassword = string.Empty;

        MarketNameUpdated?.Invoke(MarketAdi);
        MessageBox.Show("Başarıyla güncellendi", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ExecuteApplyTheme(object? parameter)
    {
        if (parameter is string color)
        {
            TemaRengi = color;
            ApplyThemeColor(color);
        }
    }

    private void ApplyThemeColor(string color)
    {
        if (ColorConverter.ConvertFromString(color) is Color accent)
        {
            Application.Current.Resources["AccentPrimary"] = new SolidColorBrush(accent);
            Application.Current.Resources["AccentBlue"] = new SolidColorBrush(accent);
        }
    }
}
