using CryptoControlCenter.Common;
using CryptoControlCenter.Common.Enums;
using Syncfusion.Windows.Shared;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;

namespace CryptoControlCenter.WPF.Dialogs
{
    /// <summary>
    /// Interaktionslogik für AddTransactionDialog.xaml
    /// </summary>
    public partial class AddTransactionDialog : ChromelessWindow
    {
        private decimal from;
        private decimal to;
        private decimal fee;
        private decimal value;
        private decimal feeValue;
        private DateTime time;
        private TransactionType type;

        public AddTransactionDialog()
        {
            this.DataContext = CryptoCenter.Instance;
            InitializeComponent();
            TransactionTime.CultureInfo = Thread.CurrentThread.CurrentCulture;
        }

        private void Boxes_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (
                //Checks if all required Elements are filled and the decimal values are parsable
                !string.IsNullOrWhiteSpace(Wallet.Text) &&
                !string.IsNullOrWhiteSpace(TransactionTime.Text) &&
                DateTime.TryParse(TransactionTime.Text, Thread.CurrentThread.CurrentCulture, out time) &&
                Enum.TryParse(Type.Text, out type) &&
                !string.IsNullOrWhiteSpace(FromAmount.Text) &&
                decimal.TryParse(FromAmount.Text, Thread.CurrentThread.CurrentCulture, out from) &&
                !string.IsNullOrWhiteSpace(FromAsset.Text) &&
                !string.IsNullOrWhiteSpace(FromWallet.Text) &&
                !string.IsNullOrWhiteSpace(ToAmount.Text) &&
                decimal.TryParse(ToAmount.Text, Thread.CurrentThread.CurrentCulture, out to) &&
                !string.IsNullOrWhiteSpace(ToAsset.Text) &&
                !string.IsNullOrWhiteSpace(ToWallet.Text) &&
                !string.IsNullOrWhiteSpace(FeeAmount.Text) &&
                decimal.TryParse(FeeAmount.Text, Thread.CurrentThread.CurrentCulture, out fee) &&
                !string.IsNullOrWhiteSpace(FeeAsset.Text) &&
                decimal.TryParse(Value.Text, Thread.CurrentThread.CurrentCulture, out value) &&
                decimal.TryParse(FeeValue.Text, Thread.CurrentThread.CurrentCulture, out feeValue))
            {
                btnDialogOk.IsEnabled = true;
            }
            else
            {
                btnDialogOk.IsEnabled = false;
            }
        }

        private void btnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            CryptoCenter.Instance.AddNewTransaction(new Common.Models.Transaction(
                Wallet.Text,
                time,
                type,
                FromAsset.Text,
                ToAsset.Text,
                from,
                to,
                type == TransactionType.Transfer ? FromWallet.Text : (type == TransactionType.BankDeposit ? "Bank" : Wallet.Text),
                type == TransactionType.Transfer ? ToWallet.Text : (type == TransactionType.BankWithdrawal ? "Bank" : Wallet.Text),
                value,
                fee,
                FeeAsset.Text,
                feeValue));
            this.DialogResult = true;
        }

        private void Type_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (Type.SelectedValue != null)
            {
                List<string> removed = new List<string>();
                foreach (object item in e.RemovedItems)
                {
                    if (item != null)
                    {
                        removed.Add(item.ToString());
                    }
                }
                //If value before change was bank-related, clear specific settings, otherwise not
                if (removed.Contains("BankDeposit"))
                {
                    FromWallet.SelectedValue = null;
                    FromWallet.SelectedItem = null;
                    FromWallet.Text = null;
                    FromWallet.IsEnabled = true;
                }
                else if (removed.Contains("BankWithdrawal"))
                {
                    ToWallet.SelectedValue = null;
                    ToWallet.SelectedItem = null;
                    ToWallet.Text = null;
                    ToWallet.IsEnabled = true;
                }
                else if(removed.Contains("Buy") || removed.Contains("Sell") || removed.Contains("Dust") || removed.Contains("Distribution"))
                {
                    FromWallet.Text = string.Empty;
                    ToWallet.Text = string.Empty;
                    FromWallet.IsEnabled = true;
                    ToWallet.IsEnabled = true;
                }

                TransactionType? type = Type.SelectedValue as TransactionType?;
                switch (type)
                {
                    case TransactionType.BankDeposit:
                        FromWallet.SelectedValue = "Bank";
                        FromWallet.SelectedItem = "Bank";
                        FromWallet.Text = "Bank";
                        FromWallet.IsEnabled = false;
                        break;
                    case TransactionType.BankWithdrawal:
                        ToWallet.SelectedValue = "Bank";
                        ToWallet.SelectedItem = "Bank";
                        ToWallet.Text = "Bank";
                        ToWallet.IsEnabled = false;
                        break;
                    case TransactionType.Buy:
                    case TransactionType.Sell:
                    case TransactionType.Dust:
                    case TransactionType.Distribution:
                        FromWallet.SelectedValue = null;
                        FromWallet.SelectedItem = null;
                        FromWallet.Text = "-";
                        FromWallet.IsEnabled = false;
                        ToWallet.SelectedValue = null;
                        ToWallet.SelectedItem = null;
                        ToWallet.Text = "-";
                        ToWallet.IsEnabled = false;
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
