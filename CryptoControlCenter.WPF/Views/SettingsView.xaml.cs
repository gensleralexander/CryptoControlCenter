using CryptoControlCenter.Common;
using System.Windows.Controls;

namespace CryptoControlCenter.WPF.Views
{
    /// <summary>
    /// Interaktionslogik für Settings.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
            CryptoCenter.Instance.IsBusy = false;
        }
    }
}
