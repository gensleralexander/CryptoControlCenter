using CryptoControlCenter.Common;
using CryptoControlCenter.Common.Helper;
using CryptoControlCenter.Common.Models.Interfaces;
using CryptoControlCenter.WPF.Helper;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;

namespace CryptoControlCenter.WPF.ViewModels
{
    public class WalletViewModel : AbstractPropertyChanged
    {
        #region Commands
        private ICommand addCommand;
        private ICommand deleteCommand;

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
        #endregion

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
            }
        }

        public WalletViewModel()
        {
        }

        private void AddExecute()
        {
            //int i = System.Random.Shared.Next();
            //CryptoCenter.Instance.CreateWallet("Test" + i, Common.Enums.Exchange.Bitstamp, "test"+ i, "test" + i);
        }

        private void DeleteExecute()
        {
            if (SelectedWallet != null)
            {
                CryptoCenter.Instance.RemoveWallet(SelectedWallet);
                SelectedWallet = null;
            }
        }
    }
}
