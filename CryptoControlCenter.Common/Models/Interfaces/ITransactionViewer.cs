using CryptoControlCenter.Common.Enums;
using System;

namespace CryptoControlCenter.Common.Models.Interfaces
{
    /// <summary>
    /// Interface for providing read-only access to Transaction Class
    /// </summary>
    public interface ITransactionViewer
    {
        /// <summary>
        /// Primary Key ID
        /// </summary>
        public int TransactionID { get; }
        /// <summary>
        /// WalletID to which the transaction is connected
        /// </summary>
        public string Wallet { get; set; }
        /// <summary>
        /// Time of Transaction
        /// </summary>
        public DateTime TransactionTime { get; }
        /// <summary>
        /// Transaction Type
        /// </summary>
        public TransactionType TransactionType { get; }
        /// <summary>
        /// Asset on Start
        /// </summary>
        public string AssetStart { get; }
        /// <summary>
        /// Asset on Destination
        /// </summary>
        public string AssetDestination { get; }
        /// <summary>
        /// Amount of Start-Asset
        /// </summary>
        public decimal? AmountStart { get; }
        /// <summary>
        /// Amount of Destination-Asset
        /// </summary>
        public decimal? AmountDestination { get; }
        /// <summary>
        /// ID of Start-Wallet
        /// </summary>
        public string LocationStart { get; }
        /// <summary>
        /// ID of Destination-Wallet
        /// </summary>
        public string LocationDestination { get; }
        /// <summary>
        /// TransactionValue in FIAT money
        /// </summary>
        public decimal? TransactionValue { get; }
        /// <summary>
        /// Fee Amount
        /// </summary>
        public decimal? FeeAmount { get; }
        /// <summary>
        /// Asset used as fee
        /// </summary>
        public string FeeAsset { get; }
        /// <summary>
        /// Fee Value in EUR
        /// </summary>
        public decimal? FeeValue { get; }
        /// <summary>
        /// Returns the Trading Pair
        /// </summary>
        public string GetTradingPair();
        /// <summary>
        /// Returns the Exchange Rate as speaking string
        /// </summary>
        public string GetExchangeRateString();
        /// <summary>
        /// Exchange Rate from Start to Destination Asset
        /// </summary>
        public decimal? GetExchangeRate();
        /// <summary>
        /// Validates a transaction, wether or not all necessary item is filled.
        /// </summary>
        /// <returns>True if valid, False if invalid</returns>
        public bool Validate();
    }
}
