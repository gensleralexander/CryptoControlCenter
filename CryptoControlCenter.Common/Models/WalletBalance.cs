using CryptoControlCenter.Common.Helper;
using CryptoControlCenter.Common.Models.Interfaces;
using System.Text;

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


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Asset: ");
            sb.AppendLine(Asset);
            sb.Append("Wallet: ");
            sb.AppendLine(Wallet);
            sb.Append("Current Amount: ");
            sb.AppendLine(CurrentAmount.ToString());
            sb.Append("Current Value: ");
            sb.AppendLine(CurrentValue.ToString());
            return sb.ToString();
        }
    }
}