using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CryptoControlCenter.WPF.Converter
{
    public class ProfitCellStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value.GetType() == typeof(decimal))
            {
                decimal profit = (decimal)value;
                if(profit > 0.0m)
                {
                    return new SolidColorBrush(Colors.Green);
                }
                else if(profit < 0.0m)
                {
                    return new SolidColorBrush(Colors.Red);
                }
            }
            return new SolidColorBrush(Colors.White);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
