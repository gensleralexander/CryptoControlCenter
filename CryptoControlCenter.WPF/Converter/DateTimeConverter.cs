using System;
using System.Globalization;
using System.Threading;
using System.Windows.Data;

namespace CryptoControlCenter.WPF.Converter
{
    public class DateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value.GetType() == typeof(DateTime))
            {
                return ((DateTime)value).ToString(Thread.CurrentThread.CurrentCulture);
            }
            else return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
