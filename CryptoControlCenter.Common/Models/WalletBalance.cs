using CryptoControlCenter.Common.Models.Interfaces;

namespace CryptoControlCenter.Common.Models
{
    public class WalletBalance : IBalanceViewer
    {
        /// <summary>
        /// Parent Wallet
        /// </summary>
        public string Wallet { get; }
        /// <summary>
        /// Asset Symbol
        /// </summary>
        public string Asset { get; }
        /// <summary>
        /// Amount of Asset
        /// </summary>
        public double CurrentAmount { get; internal set; }
        /// <summary>
        /// Value of Asset
        /// </summary>
        public double CurrentValue { get; set; }

        public WalletBalance(string wallet, string asset, double currentAmount)
        {
            Wallet = wallet;
            Asset = asset;
            CurrentAmount = currentAmount;
        }

        public WalletBalance() { }
    }
}