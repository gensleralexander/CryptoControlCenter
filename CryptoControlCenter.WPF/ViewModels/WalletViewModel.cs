using CryptoControlCenter.Common;
using CryptoControlCenter.Common.Helper;
using CryptoControlCenter.Common.Models.Interfaces;
using CryptoControlCenter.WPF.Dialogs;
using CryptoControlCenter.WPF.Helper;
using CryptoControlCenter.WPF.Views.UIModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CryptoControlCenter.WPF.ViewModels
{
    public class WalletViewModel : AbstractPropertyChanged
    {
        #region Commands
        private ICommand? addCommand;
        private ICommand? deleteCommand;
        private ICommand? syncCommand;

        public ICommand AddCommand
        {
            get
            {
                return addCommand ?? (addCommand = new RelayCommand(AddExecute));
            }
        }

        public ICommand DeleteCommand
        {
            get
            {
                return deleteCommand ?? (deleteCommand = new RelayCommand(DeleteExecute));
            }
        }

        public ICommand SyncCommand
        {
            get
            {
                return syncCommand ?? (syncCommand = new RelayCommand(SyncExecute));
            }
        }
        #endregion

        public bool ContainsMissingValues
        {
            get { return CryptoCenter.Instance.ContainsMissingValues; }
        }

        public ObservableCollection<SunburstAsset> Balances
        {
            get
            {
                if (SelectedWallet == null)
                {
                    return new ObservableCollection<SunburstAsset>();
                }
                else
                {
                    var output = new ObservableCollection<SunburstAsset>();
                    foreach (IBalanceViewer item in CryptoCenter.Instance.CurrentAssets)
                    {
                        if (item.Wallet == SelectedWallet.WalletName && item.Asset != "EUR")
                        {
                            output.Add(new SunburstAsset(item));
                        }
                    }
                    return output;
                }
            }
        }

        public List<IBalanceViewer> Positions
        {
            get
            {
                var output = new List<IBalanceViewer>();
                if (SelectedWallet != null)
                {
                    var tmp = new List<IBalanceViewer>();
                    foreach (IBalanceViewer item in CryptoCenter.Instance.CurrentAssets)
                    {
                        if (item.Wallet == SelectedWallet.WalletName && item.Asset != "EUR")
                        {
                            tmp.Add(item);
                        }
                    }
                    if (SelectedCoin == null)
                    {
                        output = tmp;
                    }
                    else
                    {
                        foreach (IBalanceViewer item in tmp)
                        {
                            if (item.Asset == SelectedCoin)
                            {
                                output.Add(item);
                            }
                        }
                    }
                    output.Sort();
                }
                return output;
            }
        }

        public ObservableCollection<IExchangeWalletViewer> Wallets
        {
            get
            {
                return CryptoCenter.Instance.ExchangeWallets;
            }
        }

        private IExchangeWalletViewer? selectedWallet;
        public IExchangeWalletViewer? SelectedWallet
        {
            get
            {
                return selectedWallet;
            }
            set
            {
                selectedWallet = value;
                OnPropertyChanged();
                OnPropertyChanged("Balances");
                OnPropertyChanged("ContainsMissingValues");
                OnPropertyChanged("TotalBalance");
                SelectedCoin = null;
            }
        }
        public string? TotalBalance
        {
            get
            {
                double total = 0.0;
                foreach (SunburstAsset balance in Balances)
                {
                    total += balance.CurrentValue;
                }
                return String.Format("{0:0,0.00 €}", total);
            }
        }

        private string? selectedCoin;
        public string? SelectedCoin
        {
            get
            {
                return selectedCoin;
            }
            set
            {
                selectedCoin = value;
                OnPropertyChanged();
                OnPropertyChanged("Positions");
            }
        }

        public WalletViewModel()
        {
        }

        private void AddExecute()
        {
            new AddWalletDialog().ShowDialog();
        }

        private void DeleteExecute()
        {
            if (SelectedWallet != null)
            {
                CryptoCenter.Instance.RemoveWallet(SelectedWallet);
                SelectedWallet = null;
            }
        }

        private void SyncExecute()
        {
            CryptoCenter.Instance.SynchronizeWallets();
        }
    }
}
