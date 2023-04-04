using System.Globalization;
using System.Threading;
using System.Windows;

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
        }
    }
}
