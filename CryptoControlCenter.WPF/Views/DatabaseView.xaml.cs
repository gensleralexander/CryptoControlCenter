using CryptoControlCenter.Common;
using CryptoControlCenter.Common.Enums;
using CryptoControlCenter.Common.Models.Interfaces;
using CryptoControlCenter.WPF.Dialogs;
using CryptoControlCenter.WPF.Resources;
using Microsoft.Win32;
using Syncfusion.UI.Xaml.Grid;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CryptoControlCenter.WPF.Views
{
    /// <summary>
    /// Interaktionslogik für DatabaseView.xaml
    /// </summary>
    public partial class DatabaseView : UserControl
    {
        public DatabaseView()
        {
            DataContext = CryptoCenter.Instance;
            InitializeComponent();
            Task.Run(() =>
            {
                Task.Delay(2000); // Wait till Syncfusion UI is loaded
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    TransactionsGrid.Visibility = Visibility.Visible;
                }));
                CryptoCenter.Instance.IsBusy = false;
            });
        }

        public bool FilterRows(object o)
        {
            var item = o as ITransactionViewer;
            if (item != null)
            {
                if (string.IsNullOrWhiteSpace(item.AssetStart)) return true;
                if (string.IsNullOrWhiteSpace(item.AssetDestination)) return true;
                if (string.IsNullOrWhiteSpace(item.FeeAsset) && item.TransactionType != TransactionType.Distribution && item.TransactionType != TransactionType.BankDeposit && !(item.TransactionType == TransactionType.Transfer && item.Wallet == item.LocationDestination)) return true;
                if (string.IsNullOrWhiteSpace(item.LocationStart)) return true;
                if (string.IsNullOrWhiteSpace(item.LocationDestination)) return true;
                if (item.AmountStart == 0.0m) return true;
                if (item.AmountDestination == 0.0m) return true;
                if (item.FeeAmount == 0.0m && item.TransactionType != TransactionType.Distribution && item.TransactionType != TransactionType.BankDeposit && !(item.TransactionType == TransactionType.Transfer && item.Wallet == item.LocationDestination)) return true;
                if ((item.TransactionValue == 0.0m && item.TransactionType != TransactionType.Distribution) || item.TransactionValue == -1.0m) return true;
                if (item.FeeValue == -1.0m) return true;
            }
            return false;
        }

        private void Filter_Click(object sender, RoutedEventArgs e)
        {
            if (FilterBox.IsChecked == true)
            {
                TransactionsGrid.SortColumnDescriptions.Clear();
                TransactionsGrid.View.Filter = FilterRows;
                TransactionsGrid.View.RefreshFilter();
            }
            else
            {
                TransactionsGrid.SortColumnDescriptions.Clear();
                TransactionsGrid.View.Filter = null;
                TransactionsGrid.View.RefreshFilter();
            }
        }

        private void LoadMissingValues_Click(object sender, RoutedEventArgs e)
        {
            var continueResult = MessageBox.Show(Strings.LoadMissingWarning + Environment.NewLine + Environment.NewLine + Strings.Continue, Strings.Warning, MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (continueResult == MessageBoxResult.Yes)
            {
                CryptoCenter.Instance.LoadMissingTransactionValues();
            }
        }

        private async void Generate_Click(object sender, RoutedEventArgs e)
        {
            var pickerResult = new SelectYearDialog(DateTime.UtcNow.Year - 1);
            if (pickerResult.ShowDialog() == true)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Excel (*.xlsx)|*.xlsx";
                saveFileDialog.FileName = "Steuerreport_" + DateTime.UtcNow.ToString("yyyy-MM-dd") + ".xlsx";
                string lastPath = Properties.Settings.Default.LastAccessFilePath;
                if (!string.IsNullOrWhiteSpace(lastPath))
                {
                    saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                }
                else
                {
                    saveFileDialog.InitialDirectory = lastPath;
                }
                if (saveFileDialog.ShowDialog() == true)
                {
                    Properties.Settings.Default.LastAccessFilePath = Path.GetDirectoryName(saveFileDialog.FileName);
                    try
                    {
                        await Common.DocumentGenerator.GenerateCryptoTaxReport(saveFileDialog.FileName, pickerResult.Year);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void TransactionsGrid_CurrentCellBeginEdit(object sender, CurrentCellBeginEditEventArgs e)
        {
        }

        private void TransactionsGrid_CurrentCellEndEdit(object sender, CurrentCellEndEditEventArgs e)
        {
        }
    }
}