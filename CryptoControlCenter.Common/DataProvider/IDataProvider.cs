using CryptoControlCenter.Common.Models.Interfaces;
using System;
using System.Threading.Tasks;

namespace CryptoControlCenter.Common.DataProvider
{
    /// <summary>
    /// Interface definition for DataProviders
    /// </summary>
    public interface IDataProvider
    {
        /// <summary>
        /// Linked Wallet
        /// </summary>
        public IExchangeWalletViewer Wallet { get; }
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