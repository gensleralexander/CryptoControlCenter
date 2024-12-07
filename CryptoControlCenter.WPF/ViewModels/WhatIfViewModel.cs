using CryptoControlCenter.Common;
using CryptoControlCenter.Common.Helper;
using CryptoControlCenter.Common.Models.Interfaces;
using CryptoControlCenter.WPF.Helper;
using CryptoControlCenter.WPF.Views.UIModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace CryptoControlCenter.WPF.ViewModels
{
    public class WhatIfViewModel : AbstractPropertyChanged
    {
        private ICommand? resetCommand;
        public ICommand ResetCommand
        {
            get
            {
                return resetCommand ?? (resetCommand = new RelayCommand(ResetExecute));
            }
        }

        public bool ContainsMissingValues
        {
            get { return CryptoCenter.Instance.ContainsMissingValues; }
        }

        public ObservableCollection<WhatIfAsset> Balances
        {
            get
            {
                var output = new ObservableCollection<WhatIfAsset>();
                if (SelectedWallet == null)
                {
                    foreach (IBalanceViewer item in CryptoCenter.Instance.CurrentAssets)
                    {
                        if (item.Asset != "EUR")
                        {
                            if(output.Any(x => x.Asset == item.Asset))
                            {
                                var existing = output.First(x => x.Asset == item.Asset);
                                existing.CurrentAmount += item.CurrentAmount;
                                existing.CurrentValue += item.CurrentValue;
                                existing.CurrentValueOnBuyRate += item.CurrentValueOnBuyRate;
                            }
                            else output.Add(new WhatIfAsset(item));
                        }
                    }
                }
                else
                {
                    foreach (IBalanceViewer item in CryptoCenter.Instance.CurrentAssets)
                    {
                        if (item.Wallet == SelectedWallet.WalletName && item.Asset != "EUR")
                        {
                            if (output.Any(x => x.Asset == item.Asset))
                            {
                                var existing = output.First(x => x.Asset == item.Asset);
                                existing.CurrentAmount += item.CurrentAmount;
                                existing.CurrentValue += item.CurrentValue;
                                existing.CurrentValueOnBuyRate += item.CurrentValueOnBuyRate;
                            }
                            else output.Add(new WhatIfAsset(item));
                        }
                    }
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
            }
        }

        public string? TotalBalance
        {
            get
            {
                decimal total = 0.0m;
                foreach (WhatIfAsset balance in Balances)
                {
                    total += balance.CurrentValue;
                }
                return String.Format("{0:0,0.00 €}", total);
            }
        }

        public WhatIfViewModel()
        {
                
        }

        private void ResetExecute()
        {
            OnPropertyChanged("Balances");
        }
    }
}
