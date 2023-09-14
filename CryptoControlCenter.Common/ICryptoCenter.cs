using CryptoControlCenter.Common.Enums;
using CryptoControlCenter.Common.Models;
using CryptoControlCenter.Common.Models.Interfaces;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace CryptoControlCenter.Common
{
    /// <summary>
    /// Interface for the singleton facade class CryptoCenter.
    /// </summary>
    public interface ICryptoCenter
    {
        /// <summary>
        /// List of all logs
        /// </summary>
        public ObservableCollection<ILogEntry> Logs { get; }
        /// <summary>
        /// List of all registered Wallets providing Asset access
        /// </summary>
        public ObservableCollection<IExchangeWalletViewer> ExchangeWallets { get; }
        /// <summary>
        /// List of all transactions
        /// </summary>
        public ObservableCollection<ITransactionViewer> Transactions { get; }
        /// <summary>
        /// Returns all current assets. The current value will be updated every 5 minutes.
        /// </summary>
        public List<IBalanceViewer> CurrentAssets { get; }
        /// <summary>
        /// Indicates the status of the queue
        /// </summary>
        public bool QueueRunning { get; }
        /// <summary>
        /// Task to create a new Wallet and adds them to the Lists
        /// </summary>
        /// <param name="walletName">Name of the new Wallet</param>
        /// <param name="exchange">Exchange used</param>
        /// <param name="exchangeApiKey">API Key for the Exchange</param>
        /// <param name="exchangeApiSecret">API Secret for the Exchange</param>
        /// <exception cref="ArgumentException">Thrown, when API credentials are empty or whitespace.</exception>
        /// <exception cref="InvalidOperationException">Thrown, when Wallet Name or API credentials are already in use.</exception>
        public Task CreateWallet(string walletName, Exchange exchange, string exchangeApiKey, string exchangeApiSecret);
        /// <summary>
        /// Removes an Agent and the connected wallet. Gets executed via internal TaskQueue to avoid conflicts with existing wallet update tasks.
        /// </summary>
        /// <param name="wallet">Wallet to be deleted</param>
        public void RemoveWallet(IExchangeWalletViewer wallet);
        /// <summary>
        /// Checks Database for Transactions that miss values for TransferValue and FeeValue and loads them via various web sources
        /// </summary>
        public void LoadMissingTransactionValues();
    }
}
