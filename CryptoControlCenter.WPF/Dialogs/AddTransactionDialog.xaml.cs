using Syncfusion.Windows.Shared;

namespace CryptoControlCenter.WPF.Dialogs
{
    /// <summary>
    /// Interaktionslogik für AddTransactionDialog.xaml
    /// </summary>
    public partial class AddTransactionDialog : ChromelessWindow
    {
        public AddTransactionDialog()
        {
            InitializeComponent();
        }

        private void Boxes_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if(
                !string.IsNullOrWhiteSpace(Wallet.Text) &&
                !string.IsNullOrWhiteSpace(TransactionTime.Text) &&
                !string.IsNullOrWhiteSpace(Type.Text) &&
                !string.IsNullOrWhiteSpace(FromAmount.Text) &&
                !string.IsNullOrWhiteSpace(FromAsset.Text) &&
                !string.IsNullOrWhiteSpace(FromWallet.Text) &&
                !string.IsNullOrWhiteSpace(ToAmount.Text) &&
                !string.IsNullOrWhiteSpace(ToAsset.Text) &&
                !string.IsNullOrWhiteSpace(ToWallet.Text) &&
                !string.IsNullOrWhiteSpace(FeeAmount.Text) &&
                !string.IsNullOrWhiteSpace(FeeAsset.Text))
            {
                btnDialogOk.IsEnabled = true;
            }
            else
            {
                btnDialogOk.IsEnabled = false;
            }
        }
    }
}
