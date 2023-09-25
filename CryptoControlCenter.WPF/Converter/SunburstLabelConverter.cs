using System;
using System.Globalization;
using System.Windows.Data;

namespace CryptoControlCenter.WPF.Converter
{
    public class SunburstLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                bool success = decimal.TryParse(value.ToString(), out decimal result);
                if (success)
                {
                    //if (Properties.Settings.Default.CurrencyIsDollar)
                    //{
                    //    return String.Format("{0:0.##}", result) + " $";
                    //}
                    //else return String.Format("{0:0.##}", result) + " €";
                    return String.Format("{0:0.##}", result) + " €"; //only support for EUR atm
                }
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
