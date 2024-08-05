using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using CryptoControlCenter.WPF.Resources;

namespace CryptoControlCenter.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(WPF.Properties.Settings.Default.LanguageCode);
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(""); //Add your Syncfusion License here
            Syncfusion.SfSkinManager.SfSkinManager.ApplyStylesOnApplication = true;
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Exception ex = (Exception)e.ExceptionObject;
                MessageBox.Show(Strings.UnhandledException + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            };
        }
    }
}
