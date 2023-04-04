using CryptoControlCenter.Common.Enums;
using CryptoControlCenter.Common.Helper;
using CryptoControlCenter.Common.Models;
using CryptoControlCenter.Common.Models.Interfaces;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;

namespace CryptoControlCenter.WPF.MockData
{
    public class MockViewModel : AbstractPropertyChanged
    {
        public MockViewModel() { }


        private IExchangeWalletViewer selectedWallet;
        public IExchangeWalletViewer SelectedWallet
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

        private ObservableCollection<IBalanceViewer> balances
        {
            get
            {
                return new ObservableCollection<IBalanceViewer>
                {
                    new WalletBalance("Test", "BTC", 500.0) { CurrentValue = 500.0},
                    new WalletBalance("Test", "ETH", 200.0) { CurrentValue = 200.0},
                    new WalletBalance("Test", "XRP", 300.0) { CurrentValue = 300.0},
                    new WalletBalance("Test2", "BTC", 100.0) { CurrentValue = 100.0},
                    new WalletBalance("Test2", "ETH", 1000.0) { CurrentValue = 1000.0},
                    new WalletBalance("Test2", "XRP", 800.0) { CurrentValue = 800.0},
                    new WalletBalance("Test3", "ETH", 300.0) { CurrentValue = 300.0},
                    new WalletBalance("Test3", "ONT", 2000.0) { CurrentValue = 2000.0},
                    new WalletBalance("Test3", "DOT", 300.0) { CurrentValue = 300.0},
                };
            }
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
                    var source = CollectionViewSource.GetDefaultView(balances);
                    source.Filter = p =>
                    {
                        return ((IBalanceViewer)p).Wallet == SelectedWallet.WalletName;
                    };
                    return source;
                }
            }
        }

        public ObservableCollection<IExchangeWalletViewer> Wallets
        {
            get
            {
                return new ObservableCollection<IExchangeWalletViewer>
                {
                    new ExchangeWallet("Test", Exchange.Binance, 0),
                    new ExchangeWallet("Test2", Exchange.Bitstamp, 1),
                    new ExchangeWallet("Test3", Exchange.Binance, 2)
                };
            }
        }
    }
}