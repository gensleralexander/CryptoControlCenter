using CryptoControlCenter.Common;
using CryptoControlCenter.Common.Enums;
using Microsoft.Win32;
using Syncfusion.Windows.Shared;
using System;
using System.IO;
using System.Windows;

namespace CryptoControlCenter.WPF.Dialogs
{
    public partial class AddWalletDialog : ChromelessWindow
    {
        Exchange? exchange;
        string lastPath = string.Empty;

        public AddWalletDialog()
        {
            lastPath = Properties.Settings.Default.LastAccessFilePath;
            InitializeComponent();
        }

        private void btnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DateTime startingPoint = new DateTime(2010, 1, 1,0,0,0);
                if(StartingPointBox.Value != null)
                {
                    startingPoint = (DateTime)StartingPointBox.Value;
                }
                switch (exchange)
                {
                    case Exchange.Generic:
                        CryptoCenter.Instance.CreateWallet(NameBox.Text, Exchange.Generic);
                        this.DialogResult = true;
                        break;
                    case Exchange.Binance:
                        CryptoCenter.Instance.CreateWallet(NameBox.Text, Exchange.Binance, TransactionsBox.Text, DepWithBox.Text, DistributionsBox.Text, startingPoint);
                        this.DialogResult = true;
                        break;
                    case Exchange.Bitstamp:
                        CryptoCenter.Instance.CreateWallet(NameBox.Text, Exchange.Bitstamp, KeyBox.Text, SecretBox.Text, startingPoint);
                        this.DialogResult = true;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                ErrorText.Text = ex.Message;
                ErrorText.Visibility = Visibility.Visible;
            }
        }

        private string Choose()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "*.csv|*.csv";
            if (!string.IsNullOrWhiteSpace(lastPath))
            {
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            else
            {
                openFileDialog.InitialDirectory = lastPath;
            }
            if (openFileDialog.ShowDialog() == true)
            {
                Properties.Settings.Default.LastAccessFilePath = Path.GetDirectoryName(openFileDialog.FileName);
                return openFileDialog.FileName;
            }
            else return string.Empty;
        }
        private void ChooseTrans_Click(object sender, RoutedEventArgs e)
        {
            string path = Choose();
            if (!string.IsNullOrWhiteSpace(path) && path.EndsWith(".csv"))
            {
                TransactionsBox.Text = path;
            }
        }
        private void ChooseDepWith_Click(object sender, RoutedEventArgs e)
        {
            string path = Choose();
            if (!string.IsNullOrWhiteSpace(path) && path.EndsWith(".csv"))
            {
                DepWithBox.Text = path;
            }
        }
        private void ChooseDistri_Click(object sender, RoutedEventArgs e)
        {
            string path = Choose();
            if (!string.IsNullOrWhiteSpace(path) && path.EndsWith(".csv"))
            {
                DistributionsBox.Text = path;
            }
        }

        private void Binance_Click(object sender, RoutedEventArgs e)
        {
            exchange = Exchange.Binance;
            TransText.Visibility = Visibility.Visible;
            TransactionsBox.Visibility = Visibility.Visible;
            TransButton.Visibility = Visibility.Visible;
            DepWithBox.Visibility = Visibility.Visible;
            DepWithButton.Visibility = Visibility.Visible;
            DepWithText.Visibility = Visibility.Visible;
            DistributionsBox.Visibility = Visibility.Visible;
            DistriButton.Visibility = Visibility.Visible;
            DistriText.Visibility = Visibility.Visible;
            SecretBox.Visibility = Visibility.Collapsed;
            SecretText.Visibility = Visibility.Collapsed;
            KeyBox.Visibility = Visibility.Collapsed;
            KeyText.Visibility = Visibility.Collapsed;
            ErrorText.Visibility = Visibility.Collapsed;
            BitstampButton.IsEnabled = true;
            BinanceButton.IsEnabled = false;
            GenericButton.IsEnabled = true;
            if (!string.IsNullOrWhiteSpace(NameBox.Text))
            {
                btnDialogOk.IsEnabled = true;
            }
        }

        private void Bitstamp_Click(object sender, RoutedEventArgs e)
        {
            exchange = Exchange.Bitstamp;
            SecretBox.Visibility = Visibility.Visible;
            SecretText.Visibility = Visibility.Visible;
            KeyBox.Visibility = Visibility.Visible;
            KeyText.Visibility = Visibility.Visible;
            TransText.Visibility = Visibility.Collapsed;
            TransactionsBox.Visibility = Visibility.Collapsed;
            TransButton.Visibility = Visibility.Collapsed;
            DepWithBox.Visibility = Visibility.Collapsed;
            DepWithButton.Visibility = Visibility.Collapsed;
            DepWithText.Visibility = Visibility.Collapsed;
            DistributionsBox.Visibility = Visibility.Collapsed;
            DistriButton.Visibility = Visibility.Collapsed;
            DistriText.Visibility = Visibility.Collapsed;
            ErrorText.Visibility = Visibility.Collapsed;
            BitstampButton.IsEnabled = false;
            BinanceButton.IsEnabled = true;
            GenericButton.IsEnabled = true;
            if (!string.IsNullOrWhiteSpace(NameBox.Text))
            {
                btnDialogOk.IsEnabled = true;
            }
        }

        private void GenericButton_Click(object sender, RoutedEventArgs e)
        {
            exchange = Exchange.Generic;
            SecretBox.Visibility = Visibility.Collapsed;
            SecretText.Visibility = Visibility.Collapsed;
            KeyBox.Visibility = Visibility.Collapsed;
            KeyText.Visibility = Visibility.Collapsed;
            TransText.Visibility = Visibility.Collapsed;
            TransactionsBox.Visibility = Visibility.Collapsed;
            TransButton.Visibility = Visibility.Collapsed;
            DepWithBox.Visibility = Visibility.Collapsed;
            DepWithButton.Visibility = Visibility.Collapsed;
            DepWithText.Visibility = Visibility.Collapsed;
            DistributionsBox.Visibility = Visibility.Collapsed;
            DistriButton.Visibility = Visibility.Collapsed;
            DistriText.Visibility = Visibility.Collapsed;
            ErrorText.Visibility = Visibility.Collapsed;
            BitstampButton.IsEnabled = true;
            BinanceButton.IsEnabled = true;
            GenericButton.IsEnabled = false;
            if (!string.IsNullOrWhiteSpace(NameBox.Text))
            {
                btnDialogOk.IsEnabled = true;
            }
        }

        private void NameBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                btnDialogOk.IsEnabled = false;
            }
            else if (exchange != null)
            {
                btnDialogOk.IsEnabled = true;
            }
        }
    }
}
