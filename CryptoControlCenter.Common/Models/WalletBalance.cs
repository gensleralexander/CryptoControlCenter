using CryptoControlCenter.Common.Helper;
using CryptoControlCenter.Common.Models.Interfaces;

namespace CryptoControlCenter.Common.Models
{
    public class WalletBalance : AbstractPropertyChanged, IBalanceViewer
    {
        private string wallet;
        private string asset;
        private double currentAmount;
        private double currentValue;
        private bool isTaxfree;

        /// <inheritdoc />
        public string Wallet { get { return wallet; } }
        /// <inheritdoc />
        public string Asset { get { return asset; } }
        /// <inheritdoc />
        public double CurrentAmount
        {
            get
            {
                return currentAmount;
            }
            internal set
            {
                currentAmount = value;
                OnPropertyChanged();
            }
        }
        /// <inheritdoc />
        public double CurrentValue
        {
            get
            {
                return currentValue;
            }
            internal set
            {
                currentValue = value;
                OnPropertyChanged();
            }
        }
        /// <inheritdoc />
        public bool IsTaxfree { get { return isTaxfree; } }
        /// <inheritdoc />
        public string TaxfreeString
        {
            get
            {
                if (IsTaxfree)
                {
                    return Resources.Strings.Taxfree;
                }
                else return Resources.Strings.Not + " " + Resources.Strings.Taxfree;
            }
        }

        public WalletBalance(string _wallet, string _asset, double _currentAmount, bool _isTaxfree, double _currentValue = 0.0)
        {
            wallet = _wallet;
            asset = _asset;
            currentAmount = _currentAmount;
            currentValue = _currentValue;
            isTaxfree = _isTaxfree;
        }

        public WalletBalance() { }
    }
}