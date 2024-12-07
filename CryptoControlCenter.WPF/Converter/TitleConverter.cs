using CryptoControlCenter.WPF.Resources;
using System;
using System.Globalization;
using System.Windows.Data;

namespace CryptoControlCenter.WPF.Converter
{
    internal class TitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case "Wallets":
                    return "CryptoControlCenter - Wallets";
                case "What If":
                    return "CryptoControlCenter - " + Strings.WhatIf;
                case "Database":
                    return "CryptoControlCenter - " + Strings.Database;
                case "Settings":
                    return "CryptoControlCenter - " + Strings.Settings;
                case "Info":
                    return "CryptoControlCenter - Info";
                default:
                    return "CryptoControlCenter";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
