using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using OmniMarket.Data;
using OmniMarket.Models;
using OmniMarket.ViewModels;
using OmniMarket.Views;

namespace OmniMarket;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Global Exception Handler (Ani çökmeleri yakalamak için)
        this.DispatcherUnhandledException += (s, args) =>
        {
            System.IO.File.WriteAllText("crash.log", $"Dispatcher Çökmesi: {args.Exception}");
            MessageBox.Show($"Kritik Çökme Engellendi!\n\nHata: {args.Exception.Message}\n\nDetay: {args.Exception.InnerException?.Message}\n\nStack:\n{args.Exception.StackTrace}",
                            "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true; // Uygulamanın kapanmasını önle
        };

        AppDomain.CurrentDomain.UnhandledException += (s, args) =>
        {
            System.IO.File.WriteAllText("crash.log", $"AppDomain Çökmesi: {args.ExceptionObject}");
        };

        System.Threading.Tasks.TaskScheduler.UnobservedTaskException += (s, args) =>
        {
            System.IO.File.WriteAllText("crash.log", $"Task Çökmesi: {args.Exception}");
            args.SetObserved();
        };

        // Veritabanını oluştur / güncelle + demo veri
        using (var db = new AppDbContext())
        {
            db.Database.EnsureCreated();

            // Ef Core EnsureCreated() var olan veritabanına yeni tablo eklemediği için
            // Logs tablosunu eğer yoksa raw SQL ile oluşturuyoruz (PostgreSQL).
            try
            {
                db.Database.ExecuteSqlRaw(@"
                    CREATE TABLE IF NOT EXISTS ""Logs"" (
                        ""Id"" uuid NOT NULL,
                        ""MarketId"" integer NOT NULL,
                        ""KullaniciAdi"" character varying(100) NOT NULL,
                        ""IslemTipi"" integer NOT NULL,
                        ""Detay"" character varying(500) NOT NULL,
                        ""Tarih"" timestamp with time zone NOT NULL,
                        CONSTRAINT ""PK_Logs"" PRIMARY KEY (""Id""),
                        CONSTRAINT ""FK_Logs_Markets_MarketId"" FOREIGN KEY (""MarketId"")
                            REFERENCES ""Markets"" (""Id"") ON DELETE CASCADE
                    );
                ");
            }
            catch { /* Hata görmezden geliniyor (Zaten varsa vs.) */ }

            // Yeni ürün alanları — mevcut DB'ye ALTER TABLE ile ekleniyor
            try
            {
                db.Database.ExecuteSqlRaw(@"ALTER TABLE ""Products"" ADD COLUMN IF NOT EXISTS ""DiscountRate"" numeric(5,2) NULL;");
            }
            catch { }
            try
            {
                db.Database.ExecuteSqlRaw(@"ALTER TABLE ""Products"" ADD COLUMN IF NOT EXISTS ""ReturnRequested"" boolean NOT NULL DEFAULT FALSE;");
            }
            catch { }

            // Log tablosuna audit sütunları — EskiDeger / YeniDeger
            try
            {
                db.Database.ExecuteSqlRaw(@"ALTER TABLE ""Logs"" ADD COLUMN IF NOT EXISTS ""EskiDeger"" character varying(500) NULL;");
            }
            catch { }
            try
            {
                db.Database.ExecuteSqlRaw(@"ALTER TABLE ""Logs"" ADD COLUMN IF NOT EXISTS ""YeniDeger"" character varying(500) NULL;");
            }
            catch { }

            DataSeeder.Seed(db);
            DataSeeder.FixEmptySupplierIds(db);
            DataSeeder.UpdateAllProductExpiryDates(db);
        }

        // Login ekranını göster
        ShowLogin();
    }

    private void ShowLogin()
    {
        var loginView = new LoginView();
        var loginVm = (LoginViewModel)loginView.DataContext;

        loginVm.LoginSucceeded += market => ShowMainWindow(market, loginView);
        loginVm.NavigateToRegister += () => ShowRegister(loginView);

        var loginWindow = new Window
        {
            Title = "OmniMarket – Giriş",
            Content = loginView,
            Width = 600,
            Height = 820,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(15, 23, 42)),
            ResizeMode = ResizeMode.NoResize,
            WindowStyle = WindowStyle.SingleBorderWindow
        };

        loginWindow.Show();
    }

    private void ShowRegister(UserControl previousView)
    {
        var parentWindow = Window.GetWindow(previousView);

        var registerView = new RegisterView();
        var registerVm = (RegisterViewModel)registerView.DataContext;

        registerVm.NavigateToLogin += () =>
        {
            if (parentWindow != null)
            {
                parentWindow.Content = new LoginView();
                var newLoginVm = (LoginViewModel)((LoginView)parentWindow.Content).DataContext;
                newLoginVm.LoginSucceeded += market => ShowMainWindow(market, (LoginView)parentWindow.Content);
                newLoginVm.NavigateToRegister += () => ShowRegister((LoginView)parentWindow.Content);
            }
        };

        if (parentWindow != null)
        {
            parentWindow.Content = registerView;
        }
    }

    private void ShowMainWindow(Market market, UserControl loginView)
    {
        var loginWindow = Window.GetWindow(loginView);

        var mainVm = new MainViewModel();
        mainVm.Initialize(market);

        mainVm.LoggedOut += () =>
        {
            foreach (Window w in Current.Windows)
            {
                if (w is MainWindow)
                {
                    w.Close();
                    break;
                }
            }
            ShowLogin();
        };

        var mainWindow = new MainWindow(mainVm);
        mainWindow.Show();

        loginWindow?.Close();
    }
}
