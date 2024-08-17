using CryptoControlCenter.Common;
using CryptoControlCenter.Common.Models;
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
                Task.Delay(2000); // Wait 2 seconds for Syncfusion UI to load all data
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    TransactionsGrid.Visibility = Visibility.Visible;
                }));
                CryptoCenter.Instance.IsBusy = false;
            });
        }

        /// <summary>
        /// Filters the database view for invalid transactions. True = invalid and shown in table.
        /// </summary>
        public bool FilterRows(object o)
        {
            var item = o as Transaction;
            if (item != null)
            {
                return !item.Validate();
            }
            return true;
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
                        await DocumentGenerator.GenerateCryptoTaxReport(saveFileDialog.FileName, pickerResult.Year);
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

        private void AddTransaction_Click(object sender, RoutedEventArgs e)
        {
            new AddTransactionDialog().ShowDialog();
        }
        private void DeleteTransaction_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(Strings.DeleteTransactionConfirmation, Strings.Continue, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                var transaction = TransactionsGrid.SelectedItem as Transaction;
                if (transaction != null)
                {
                    CryptoCenter.Instance.DeleteTransaction(transaction);
                }
            }
        }

        private void TransactionsGrid_SelectionChanged(object sender, GridSelectionChangedEventArgs e)
        {
            if (sender != null)
            {
                var grid = sender as SfDataGrid;
                if (grid.SelectedItem != null)
                {
                    DeleteButton.IsEnabled = true;
                }
                else DeleteButton.IsEnabled = false;
            }
        }
    }
}