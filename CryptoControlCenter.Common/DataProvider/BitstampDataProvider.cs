using Bitstamp.Net;
using Bitstamp.Net.Enums;
using Bitstamp.Net.Interfaces;
using Bitstamp.Net.Objects;
using CryptoControlCenter.Common.Database;
using CryptoControlCenter.Common.Enums;
using CryptoControlCenter.Common.Models;
using CryptoControlCenter.Common.Models.Interfaces;
using CryptoExchange.Net.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoControlCenter.Common.DataProvider
{
    /// <summary>
    /// Data Provider for Bitstamp Wallets
    /// </summary>
    public class BitstampDataProvider : IDataProvider
    {
        private IBitstampClient Client;
        /// <inheritdoc />
        public IExchangeWalletViewer Wallet { get; }


        public BitstampDataProvider(IExchangeWalletViewer wallet)
        {
            Wallet = wallet;
            Client = new BitstampClient();
            SecuredCredentials credentials = SQLiteDatabaseManager.Database.FindAsync<SecuredCredentials>(x => x.ID == Wallet.SecureCredentialID).Result;
            if (credentials != null)
            {
                Client.SetApiCredentials(new ApiCredentials(credentials.Key, credentials.GetDecryptedSecret()));
            }
        }

        /// <inheritdoc/>
        public void ImportFromCSV(string csvFilePathTransactions, string csvFilePathWithdrawalDeposits, string csvFilePathDistribution)
        {
            throw new NotImplementedException("CSV Imports for Bitstamp are currently not implemented.");
            //TODO
        }

        /// <inheritdoc />
        public async Task SynchronizeWallet(DateTime? startingPoint = null)
        {
            //Delays in this method are used to prevent API abuse.
#if DEBUG
            Console.WriteLine("Bitstamp Provider requested synchronization. ID " + Wallet.WalletName);
#endif
            DateTime startTime;
            DateTime businessStartBitstamp = new DateTime(2011, 8, 1);
            if (startingPoint == null || startingPoint < businessStartBitstamp)
            {
                startTime = businessStartBitstamp;
            }
            else if (startingPoint > DateTime.UtcNow)
            {
                throw new ArgumentException("Parameter startingPoint cannot be in the future.");
            }
            else
            {
                startTime = (DateTime)startingPoint;
            }
            List<Transaction> transactions = new List<Transaction>();

            #region User Transactions
            IEnumerable<BitstampUserTransaction> userTransactions = new List<BitstampUserTransaction>();
            var userTransactionsResult = await Client.Api.Private.GetUserTransactionsAsync(null, 0, 1000, SortingOrder.Ascending, null, null); //since_timestamp parameter not working on Bitstamp. Transactions before startingPoint parameter will be dropped
            if (userTransactionsResult.Success)
            {
                userTransactions = userTransactionsResult.Data;
                foreach (BitstampUserTransaction userTransaction in userTransactions)
                {
                    if (userTransaction.TransactionTime > startTime)
                    {
                        if (userTransaction.ToAsset == null && userTransaction.FromAsset == null)
                        {
                            // Bitstamp API seems bugged and sometimes sends an empty transaction
                        }
                        else
                        {
                            switch (userTransaction.Type)
                            {
                                case UserTransactionType.Deposit:
                                    transactions.Add(new Transaction(Wallet.WalletName, userTransaction.TransactionTime, userTransaction.ToAsset == "EUR" ? TransactionType.BankDeposit : TransactionType.Transfer, userTransaction.ToAsset, userTransaction.ToAsset, userTransaction.ToAmount, userTransaction.ToAmount, string.Empty, Wallet.WalletName, -1.0m, 0.0m, string.Empty));
                                    break;
                                case UserTransactionType.Withdrawal:
                                    transactions.Add(new Transaction(Wallet.WalletName, userTransaction.TransactionTime, userTransaction.ToAsset == "EUR" ? TransactionType.BankWithdrawal : TransactionType.Transfer, userTransaction.FromAsset, userTransaction.FromAsset, userTransaction.FromAmount, userTransaction.FromAmount, Wallet.WalletName, string.Empty, -1.0m, userTransaction.FeeAmount, userTransaction.FeeAsset));
                                    break;
                                case UserTransactionType.MarketTrade:
                                    if (userTransaction.FromAsset != null && userTransaction.ToAsset != null)
                                    {
                                        transactions.Add(new Transaction(Wallet.WalletName, userTransaction.TransactionTime, userTransaction.IsBuyer ? TransactionType.Buy : TransactionType.Sell, userTransaction.FromAsset, userTransaction.ToAsset, userTransaction.FromAmount, userTransaction.ToAmount, Wallet.WalletName, Wallet.WalletName, -1.0m, userTransaction.FeeAmount, userTransaction.FeeAsset));
                                    }
                                    else//Bitstamp sometimes has this mini transactions that are below 0.0001 euro
                                    {
                                        transactions.Add(new Transaction(Wallet.WalletName, userTransaction.TransactionTime, userTransaction.IsBuyer ? TransactionType.Buy : TransactionType.Sell, userTransaction.FromAsset ?? "EUR", userTransaction.ToAsset ?? "EUR", userTransaction.FromAmount, userTransaction.ToAmount, Wallet.WalletName, Wallet.WalletName, -1.0m, userTransaction.FeeAmount, userTransaction.FeeAsset));
                                    }
                                    break;
                                case UserTransactionType.SubAccountTransfer:
                                case UserTransactionType.CreditedWithStakedAssets:
                                case UserTransactionType.SentAssetsToStaking:
                                case UserTransactionType.StakingReward:
                                case UserTransactionType.ReferralReward:
                                case UserTransactionType.InterAccountTransfer:
                                    CryptoCenter.InternalInstance.AddLog("BitstampAgent: No implementation for UserTransactionType " + userTransaction.Type.ToString(), Wallet.WalletName);
                                    break;
                            }
                        }
                    }
                }
                while (userTransactions.Count() == 1000)
                {
                    await Task.Delay(50);
                    long lastID = userTransactions.Last().Id;
                    userTransactionsResult = await Client.Api.Private.GetUserTransactionsAsync(null, 0, 1000, SortingOrder.Ascending, null, lastID);
                    if (userTransactionsResult.Success)
                    {
                        userTransactions = userTransactionsResult.Data;
                        foreach (BitstampUserTransaction userTransaction in userTransactions)
                        {
                            if (userTransaction.TransactionTime > startTime)
                            {
                                switch (userTransaction.Type)
                                {
                                    case UserTransactionType.Deposit:
                                        transactions.Add(new Transaction(Wallet.WalletName, userTransaction.TransactionTime, userTransaction.ToAsset == "EUR" ? TransactionType.BankDeposit : TransactionType.Transfer, userTransaction.ToAsset, userTransaction.ToAsset, userTransaction.ToAmount, userTransaction.ToAmount, string.Empty, Wallet.WalletName, -1.0m, 0.0m, string.Empty));
                                        break;
                                    case UserTransactionType.Withdrawal:
                                        transactions.Add(new Transaction(Wallet.WalletName, userTransaction.TransactionTime, userTransaction.FromAsset == "EUR" ? TransactionType.BankWithdrawal : TransactionType.Transfer, userTransaction.FromAsset, userTransaction.FromAsset, userTransaction.FromAmount, userTransaction.FromAmount, Wallet.WalletName, string.Empty, -1.0m, userTransaction.FeeAmount, userTransaction.FeeAsset));
                                        break;
                                    case UserTransactionType.MarketTrade:
                                        transactions.Add(new Transaction(Wallet.WalletName, userTransaction.TransactionTime, userTransaction.IsBuyer ? TransactionType.Buy : TransactionType.Sell, userTransaction.FromAsset, userTransaction.ToAsset, userTransaction.FromAmount, userTransaction.ToAmount, Wallet.WalletName, Wallet.WalletName, -1.0m, userTransaction.FeeAmount, userTransaction.FeeAsset));
                                        break;
                                    case UserTransactionType.SubAccountTransfer:
                                    case UserTransactionType.CreditedWithStakedAssets:
                                    case UserTransactionType.SentAssetsToStaking:
                                    case UserTransactionType.StakingReward:
                                    case UserTransactionType.ReferralReward:
                                    case UserTransactionType.InterAccountTransfer:
                                        CryptoCenter.InternalInstance.AddLog("BitstampAgent: No implementation for UserTransactionType " + userTransaction.Type.ToString(), Wallet.WalletName);
                                        break;
                                }
                            }
                        }
                    }
                    else
                    {
                        CryptoCenter.InternalInstance.AddLog(userTransactionsResult.Error.Message, Wallet.WalletName);
                    }
                }
            }
            else
            {
                CryptoCenter.InternalInstance.AddLog(userTransactionsResult.Error.Message, Wallet.WalletName);
            }
            #endregion
            #region Crypto Transactions
            IEnumerable<BitstampCryptoTransaction> cryptoTransactions = new List<BitstampCryptoTransaction>();
            int count = transactions.Where(x => x.Wallet == Wallet.WalletName && x.TransactionType == TransactionType.Transfer).Count();
            var cryptoTransactionsResult = await Client.Api.Private.GetCryptoDepositsAndWithdrawalsAsync(count, 1000, true);
            if (cryptoTransactionsResult.Success)
            {
                foreach (BitstampCryptoTransaction withdrawal in cryptoTransactionsResult.Data.Withdrawals)
                {
                    if (withdrawal.Datetime > startTime)
                    {
                        transactions.Add(new Transaction(Wallet.WalletName, withdrawal.Datetime, Enums.TransactionType.Transfer, withdrawal.Currency, withdrawal.Currency, withdrawal.Amount, withdrawal.Amount, Wallet.WalletName, string.Empty, -1.0m, 0.0m, withdrawal.Currency));
                    }
                }
                foreach (BitstampCryptoTransaction deposit in cryptoTransactionsResult.Data.Deposits)
                {
                    if (deposit.Datetime > startTime)
                    {
                        transactions.Add(new Transaction(Wallet.WalletName, deposit.Datetime, Enums.TransactionType.Transfer, deposit.Currency, deposit.Currency, deposit.Amount, deposit.Amount, string.Empty, Wallet.WalletName, -1.0m, 0.0m, deposit.Currency));
                    }
                }
            }

            #endregion

            //Pass to CryptoCenter
            transactions.Sort();
            foreach (Transaction transaction in transactions)
            {
                CryptoCenter.InternalInstance.AddNewTransaction(transaction);
            }
#if DEBUG
            Console.WriteLine("Bitstamp Wallet initialized. Name: " + Wallet.WalletName);
#endif
        }
    }
}