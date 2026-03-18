using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace OmniMarket.Helpers;

/// <summary>
/// String boş değilse Visible, boşsa Collapsed döndürür.
/// </summary>
public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return string.IsNullOrWhiteSpace(value as string) ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Değer 0 ise Visible, değilse Collapsed döndürür. (Boş durum göstergesi)
/// </summary>
public class ZeroToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int count)
            return count == 0 ? Visibility.Visible : Visibility.Collapsed;
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// SKT durumuna göre arka plan rengi döndüren converter.
/// "Expired" → Kırmızı, "Warning" → Sarı, "Normal" → Yeşil
/// </summary>
public class ExpiryStatusConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string status)
        {
            return status switch
            {
                "Expired" => new SolidColorBrush(Color.FromRgb(239, 68, 68)),   // Kırmızı
                "Warning" => new SolidColorBrush(Color.FromRgb(245, 158, 11)),  // Sarı/Turuncu
                "Normal"  => new SolidColorBrush(Color.FromRgb(34, 197, 94)),   // Yeşil
                _ => new SolidColorBrush(Colors.Gray)
            };
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// SKT durumuna göre metin döndüren converter.
/// </summary>
public class ExpiryStatusTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string status)
        {
            return status switch
            {
                "Expired" => "● SKT Geçmiş",
                "Warning" => "● SKT Yakın",
                "Normal"  => "● Normal",
                _ => "● Bilinmiyor"
            };
        }
        return "● Bilinmiyor";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Stok < 5 ise kritik uyarı rengi döndüren converter.
/// </summary>
public class StockWarningConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isLow && isLow)
        {
            return new SolidColorBrush(Color.FromRgb(239, 68, 68)); // Kırmızı
        }
        return new SolidColorBrush(Color.FromRgb(148, 163, 184)); // Normal gri
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
