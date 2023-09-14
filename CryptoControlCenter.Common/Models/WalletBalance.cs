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

        /// <summary>
        /// Parent Wallet
        /// </summary>
        public string Wallet { get { return wallet; } }
        /// <summary>
        /// Asset Symbol
        /// </summary>
        public string Asset { get { return asset; } }
        /// <summary>
        /// Amount of Asset
        /// </summary>
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
        /// <summary>
        /// Value of Asset
        /// </summary>
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
        /// <summary>
        /// Determines, wether a balance is taxfree (§23 EStG, german tax law)
        /// </summary>
        public bool IsTaxfree { get { return isTaxfree; } }

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