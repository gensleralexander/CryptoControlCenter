using CryptoControlCenter.Common;
using System.Windows.Controls;

namespace CryptoControlCenter.WPF.Views
{
    /// <summary>
    /// Interaktionslogik für InfoView.xaml
    /// </summary>
    public partial class InfoView : UserControl
    {
        public InfoView()
        {
            InitializeComponent();
            CryptoCenter.Instance.IsBusy = false;
        }
    }
}
