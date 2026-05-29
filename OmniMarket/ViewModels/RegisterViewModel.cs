using System.Windows;
using OmniMarket.Helpers;
using OmniMarket.Models;
using OmniMarket.Services;

namespace OmniMarket.ViewModels;

public class RegisterViewModel : BaseViewModel
{
    private readonly AuthService _authService = new();
    private string _marketName = string.Empty;
    private string _username = string.Empty;
    private string _password = string.Empty;
    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;

    public string MarketName
    {
        get => _marketName;
        set => SetProperty(ref _marketName, value);
    }

    public string Username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public string SuccessMessage
    {
        get => _successMessage;
        set => SetProperty(ref _successMessage, value);
    }

    public RelayCommand RegisterCommand { get; }
    public RelayCommand GoToLoginCommand { get; }

    public event Action? NavigateToLogin;

    public RegisterViewModel()
    {
        RegisterCommand = new RelayCommand(ExecuteRegister);
        GoToLoginCommand = new RelayCommand(() => NavigateToLogin?.Invoke());
    }

    private void ExecuteRegister()
    {
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(MarketName) ||
            string.IsNullOrWhiteSpace(Username) ||
            string.IsNullOrWhiteSpace(Password))
        {
            MessageBox.Show("Tüm alanlar doldurulmalıdır.", "Eksik Bilgi", MessageBoxButton.OK, MessageBoxImage.Warning);
            ErrorMessage = "Tüm alanlar doldurulmalıdır.";
            return;
        }

        if (Password.Length < 4)
        {
            MessageBox.Show("Şifre en az 4 karakter olmalıdır.", "Güvenlik Uyarısı", MessageBoxButton.OK, MessageBoxImage.Warning);
            ErrorMessage = "Şifre en az 4 karakter olmalıdır.";
            return;
        }

        try
        {
            var market = _authService.Register(MarketName, Username, Password);
            if (market != null)
            {
                MessageBox.Show("Kayıt başarılı! Giriş yapabilirsiniz.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                SuccessMessage = "Kayıt başarılı! Giriş yapabilirsiniz.";
                MarketName = string.Empty;
                Username = string.Empty;
                Password = string.Empty;
            }
            else
            {
                MessageBox.Show("Bu kullanıcı adı zaten kullanılıyor.", "Kayıt Başarısız", MessageBoxButton.OK, MessageBoxImage.Error);
                ErrorMessage = "Bu kullanıcı adı zaten kullanılıyor.";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Kayıt işlemi sırasında veritabanı hatası oluştu: {ex.Message}", "Bağlantı Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
