﻿using CryptoControlCenter.Common;
using CryptoControlCenter.Common.Helper;
using CryptoControlCenter.Common.Models.Interfaces;
using CryptoControlCenter.WPF.Dialogs;
using CryptoControlCenter.WPF.Helper;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace CryptoControlCenter.WPF.ViewModels
{
    public class WalletViewModel : AbstractPropertyChanged
    {
        #region Commands
        private ICommand addCommand;
        private ICommand deleteCommand;
        private ICommand syncCommand;

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

        public ICollectionView Balances
        {
            get
            {
                if (SelectedWallet == null)
                {
                    return CollectionViewSource.GetDefaultView(new ObservableCollection<IBalanceViewer>());
                }
                else
                {
                    var source = CollectionViewSource.GetDefaultView(CryptoCenter.Instance.CurrentAssets);
                    source.Filter = item =>
                    {
                        return ((IBalanceViewer)item).Wallet == SelectedWallet.WalletName;
                    };
                    return source;
                }
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
