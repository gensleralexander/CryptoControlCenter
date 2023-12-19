using CryptoControlCenter.Common;
using System;
using System.Reflection;

namespace CryptoControlCenter.WPF.ViewModels
{
    public class InfoViewModel
    {
        private static Version appVersion
        {
            get
            {
                return Assembly.GetEntryAssembly().GetName().Version;
            }
        }
        public static string AppVersion
        {
            get
            {
                return appVersion.ToString(4);
            }
        }
        public static int DatabaseVersion
        {
            get
            {
                return CryptoCenter.DatabaseVersion;
            }
        }
    }
}
