using Syncfusion.Windows.Shared;
using System.Windows;

namespace CryptoControlCenter.WPF.Dialogs
{
    /// <summary>
    /// Interaktionslogik für AddWalletDialog.xaml
    /// </summary>
    public partial class AddWalletDialog : ChromelessWindow
    {
        public AddWalletDialog()
        {
            InitializeComponent();
        }

        private void btnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
