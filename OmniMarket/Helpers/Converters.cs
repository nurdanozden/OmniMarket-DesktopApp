using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace OmniMarket.Helpers;



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




public class ExpiryStatusConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string status)
        {
            return status switch
            {
                "Expired" => new SolidColorBrush(Color.FromRgb(239, 68, 68)),
                "Warning" => new SolidColorBrush(Color.FromRgb(249, 115, 22)),
                "Normal"  => new SolidColorBrush(Color.FromRgb(16, 185, 129)),
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



public class StockWarningConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isLow && isLow)
        {
            return new SolidColorBrush(Color.FromRgb(249, 115, 22));
        }
        return new SolidColorBrush(Color.FromRgb(107, 114, 128));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}



public class CategoryToEmojiConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string category)
        {
            return category switch
            {
                "Süt" or "Süt & Süt Ürünleri" => "🥛",
                "Meyve & Sebze" => "🍎",
                "Fırın" => "🍞",
                "Kahvaltılık" => "🍳",
                "İçecek" or "İçecekler" => "🥤",
                "Atıştırmalık" or "Atıştırmalıklar" => "🍫",
                "Temizlik" or "Temizlik Ürünleri" => "🧼",
                "Bakliyat" or "Un & Bakliyat" => "🌾",
                "Çay & Kahve" => "☕",
                "Dondurulmuş Gıda" or "Dondurulmuş Gıdalar" => "🧊",
                _ => "📦"
            };
        }
        return "📦";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}



public class HexStringToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string hex && !string.IsNullOrWhiteSpace(hex))
        {
            try
            {
                var color = (Color)ColorConverter.ConvertFromString(hex);
                return new SolidColorBrush(color);
            }
            catch { }
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

