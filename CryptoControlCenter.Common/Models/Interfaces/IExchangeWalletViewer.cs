using CryptoControlCenter.Common.Enums;
using System;
using System.Threading.Tasks;

namespace CryptoControlCenter.Common.Models.Interfaces
{
    /// <summary>
    /// This interface providing read-only access to the ExchangeWallet-Model
    /// </summary>
    public interface IExchangeWalletViewer
    {
        /// <summary>
        /// PK WalletName
        /// </summary>
        public string WalletName { get; set; }
        /// <summary>
        /// Exchange
        /// </summary>
        public Exchange Exchange { get; }
        /// <summary>
        /// Time of last synchronization with exchange
        /// </summary>
        public DateTime? LastSync { get; }
        /// <summary>
        /// Returns a description of the Wallet.
        /// </summary>
        public string GetDescription();
        /// <summary>
        /// ID of the encrypted Credentials
        /// </summary>
        public int SecureCredentialID { get; }
        /// <summary>
        /// Synchronizes the past trades to present and place them into wallet.
        /// </summary>
        /// <param name="startingPoint">Defines a start point for data initialization.</param>
        public Task SynchronizeWallet(DateTime? startingPoint = null);
        /// <summary>
        /// Import values from CSV files
        /// </summary>
        /// <param name="csvFilePathTransactions">File Path to Transactions File</param>
        /// <param name="csvFilePathWithdrawalDeposits">File Path to Withdrawal/Deposits File</param>
        /// <param name="csvFilePathDistribution">File Path to Distribution File</param>
        /// <returns></returns>
        public void ImportFromCSV(string csvFilePathTransactions, string csvFilePathWithdrawalDeposits, string csvFilePathDistribution);
    }
}
