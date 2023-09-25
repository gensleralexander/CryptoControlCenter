using CryptoControlCenter.Common;
using System.Windows.Controls;

namespace CryptoControlCenter.WPF.Views
{
    /// <summary>
    /// Interaktionslogik für WalletView.xaml
    /// </summary>
    public partial class WalletView : UserControl
    {
        public WalletView()
        {
            InitializeComponent();
            Warning.DataContext = CryptoCenter.Instance;
        }
    }
}