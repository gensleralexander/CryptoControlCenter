using System;
using System.Globalization;
using System.Windows.Data;

namespace CryptoControlCenter.WPF.Converter
{
    public class AssetValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value.GetType() == typeof(decimal))
            {
                return ((decimal)value).ToString("C");
            }
            else if (value != null && value.GetType() == typeof(double))
            {
                return ((double)value).ToString("C");
            }
            else return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
