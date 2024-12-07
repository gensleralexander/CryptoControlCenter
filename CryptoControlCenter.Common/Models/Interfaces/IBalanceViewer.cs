using System;

namespace CryptoControlCenter.Common.Models.Interfaces
{
    public interface IBalanceViewer
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
        public decimal CurrentAmount { get; }
        /// <summary>
        /// Value of Asset
        /// </summary>
        public decimal CurrentValue { get; }
        /// <summary>
        /// Value of the current amount with the original buy price
        /// </summary>
        public decimal CurrentValueOnBuyRate { get; }
        /// <summary>
        /// Determines, wether a balance is taxfree (§23 EStG, german tax law)
        /// </summary>
        public bool IsTaxfree { get; }
        /// <summary>
        /// Returns the boolean Taxfree as string
        /// </summary>
        public string TaxfreeString { get; }
        /// <summary>
        /// DateTime, when the asset was received (through Deposit or Trade)
        /// </summary>
        public DateTime Received { get; }
        /// <summary>
        /// The current profit of the asset in %
        /// </summary>
        public decimal Profit { get; }
    }
}
