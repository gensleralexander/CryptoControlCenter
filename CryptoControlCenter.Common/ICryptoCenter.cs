﻿using CryptoControlCenter.Common.Enums;
using CryptoControlCenter.Common.Models;
using CryptoControlCenter.Common.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CryptoControlCenter.Common
{
    /// <summary>
    /// Interface for the singleton facade class CryptoCenter.
    /// </summary>
    public interface ICryptoCenter
    {
        public bool IsBusy { get; set; }
        public bool ContainsMissingValues { get; }
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
        public ObservableCollection<Transaction> Transactions { get; }
        /// <summary>
        /// Returns all current assets. The current value will be updated every 5 minutes.
        /// </summary>
        public List<IBalanceViewer> CurrentAssets { get; }
        /// <summary>
        /// Returns a list of strings with all available ExchangeWallets
        /// </summary>
        public List<string> WalletNames { get; }
        /// <summary>
        /// Indicates the status of the queue
        /// </summary>
        public bool QueueRunning { get; }
        /// <summary>
        /// Refreshes the balances/assets with the current values.
        /// </summary>
        public void RefreshBalanceValues();
        /// <summary>
        /// Method to create a new Wallet and adds them to the Lists.
        /// </summary>
        /// <param name="walletName">Name of the new Wallet</param>
        /// <param name="exchange">Exchange used</param>
        public void CreateWallet(string walletName, Exchange exchange);
        /// <summary>
        /// Method to create a new Wallet and adds them to the Lists. Uses API-sync
        /// </summary>
        /// <param name="walletName">Name of the new Wallet</param>
        /// <param name="exchange">Exchange used</param>
        /// <param name="exchangeApiKey">API Key for the Exchange</param>
        /// <param name="exchangeApiSecret">API Secret for the Exchange</param>
        /// <param name="startingPoint">Starting DateTime for API-Sync</param>
        /// <exception cref="ArgumentException">Thrown, when API credentials are empty or whitespace.</exception>
        /// <exception cref="InvalidOperationException">Thrown, when Wallet Name or API credentials are already in use.</exception>
        public void CreateWallet(string walletName, Exchange exchange, string exchangeApiKey, string exchangeApiSecret, DateTime startingPoint);
        /// <summary>
        /// Method to create a new Wallet and adds them to the Lists. Instead of API-sync, this method uses CSV-Import
        /// </summary>
        /// <param name="walletName">Name of the new Wallet</param>
        /// <param name="exchange">Exchange used</param>
        /// <param name="csvFilePathTransactions">File Path to Transactions File</param>
        /// <param name="csvFilePathWithdrawalDeposits">File Path to Withdrawal/Deposits File</param>
        /// <param name="csvFilePathDistribution">File Path to Distribution File</param>
        /// <param name="startingPoint">Starting DateTime for CSV-Sync</param>
        /// <exception cref="ArgumentException">Thrown, when csv paths are empty or whitespace.</exception>
        /// <exception cref="InvalidOperationException">Thrown, when Wallet Name is already in use.</exception>
        public void CreateWallet(string walletName, Exchange exchange, string csvFilePathTransactions, string csvFilePathWithdrawalDeposits, string csvFilePathDistribution, DateTime startingPoint);
        /// <summary>
        /// Removes an Agent and the connected wallet. Gets executed via internal TaskQueue to avoid conflicts with existing wallet update tasks.
        /// </summary>
        /// <param name="wallet">Wallet to be deleted</param>
        public void RemoveWallet(IExchangeWalletViewer wallet);
        /// <summary>
        /// Adds a new transaction to the Collection and Database
        /// </summary>
        /// <param name="transaction">Transaction to be added</param>
        public void AddNewTransaction(Transaction transaction);
        /// <summary>
        /// Deletes transaction from the Collection and Database
        /// </summary>
        /// <param name="transaction">Transaction to be deleted</param>
        public void DeleteTransaction(Transaction transaction);
        /// <summary>
        /// Synchronize all API-based Wallets
        /// </summary>
        public void SynchronizeWallets();
    }
}
