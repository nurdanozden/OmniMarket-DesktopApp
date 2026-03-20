using System.Windows;
using OmniMarket.Helpers;
using OmniMarket.Models;
using OmniMarket.Services;

namespace OmniMarket.ViewModels;

public class LoginViewModel : BaseViewModel
{
    private readonly AuthService _authService = new();
    private string _username = string.Empty;
    private string _password = string.Empty;
    private string _errorMessage = string.Empty;

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

    public RelayCommand LoginCommand { get; }
    public RelayCommand GoToRegisterCommand { get; }
    public RelayCommand TestLoginCommand { get; }

    // Events for navigation
    public event Action<Market>? LoginSucceeded;
    public event Action? NavigateToRegister;

    // PasswordBox senkronizasyonu için
    public event Action<string>? PasswordFilled;

    public LoginViewModel()
    {
        LoginCommand = new RelayCommand(ExecuteLogin);
        GoToRegisterCommand = new RelayCommand(() => NavigateToRegister?.Invoke());
        TestLoginCommand = new RelayCommand(ExecuteTestLogin);
    }

    private void ExecuteLogin()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Kullanıcı adı ve şifre boş bırakılamaz.";
            return;
        }

        var market = _authService.Login(Username, Password);
        if (market != null)
        {
            var logService = new LogService();
            logService.AddLog(market.Id, market.Username, LogType.Login, "Sisteme giriş yapıldı (Oturum Açıldı).");
            LoginSucceeded?.Invoke(market);
        }
        else
        {
            ErrorMessage = "Geçersiz kullanıcı adı veya şifre.";
        }
    }

    private void ExecuteTestLogin()
    {
        Username = "nur";
        Password = "1234";
        PasswordFilled?.Invoke("1234");
        ExecuteLogin();
    }
}

