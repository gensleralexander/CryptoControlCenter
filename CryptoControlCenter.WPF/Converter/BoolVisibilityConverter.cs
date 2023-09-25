using System;
using System.Windows.Data;
using System.Windows;
using System.Globalization;

namespace CryptoControlCenter.WPF.Converter
{
    /// <summary>
    /// Converts a boolean value to XAML.Visibility.
    /// Can be inverted by using the parameter "negative"
    /// </summary>
    public class BoolVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                if (parameter as string == "negative")
                {
                    switch ((bool)value)
                    {
                        case false:
                            return Visibility.Visible;
                        case true:
                        default:
                            return Visibility.Collapsed;
                    }
                }
                else switch ((bool)value)
                    {
                        case true:
                            return Visibility.Visible;
                        case false:
                        default:
                            return Visibility.Collapsed;
                    }
            }
            else return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
