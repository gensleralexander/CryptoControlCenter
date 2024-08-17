using Bitstamp.Net;
using Bitstamp.Net.Enums;
using Bitstamp.Net.Interfaces;
using Bitstamp.Net.Objects;
using CryptoControlCenter.Common.Database;
using CryptoControlCenter.Common.Enums;
using CryptoControlCenter.Common.Helper;
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
        List<ExchangeRateRequest> exchangeRates = new List<ExchangeRateRequest>();
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
        public void ImportFromCSV(string csvFilePathTransactions, string csvFilePathWithdrawalDeposits, string csvFilePathDistribution, DateTime? startingPoint)
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
            CryptoCenter.InternalInstance.IsBusy = true;
            List<Transaction> transactions = new List<Transaction>();
            IEnumerable<BitstampUserTransaction> userTransactions;
            #region User Transactions
            do
            {
                userTransactions = new List<BitstampUserTransaction>();
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
                                        if (userTransaction.ToAsset == "EUR")
                                        {
                                            transactions.Add(new Transaction(
                                            Wallet.WalletName,
                                            userTransaction.TransactionTime,
                                            TransactionType.BankDeposit,
                                            string.Empty, //assetStart
                                            userTransaction.ToAsset, //assetDestination
                                            null, //amountStart
                                            userTransaction.ToAmount, //amountDestination
                                            "Bank", //locationStart
                                            Wallet.WalletName, //locationDestination
                                            userTransaction.ToAmount, //value
                                            null, //feeAmount
                                            string.Empty,//feeAsset
                                            null)); //feeValue
                                        }
                                        else transactions.Add(new Transaction(
                                            Wallet.WalletName,
                                            userTransaction.TransactionTime,
                                            TransactionType.Transfer,
                                            userTransaction.ToAsset, //assetStart
                                            string.Empty, //assetDestination
                                            userTransaction.ToAmount, //amountStart
                                            null, //amountDestination
                                            string.Empty, //locationStart
                                            Wallet.WalletName, //locationDestination
                                            null, //value
                                            0.0m, //feeAmount
                                            "EUR",//feeAsset
                                            0.0m)); //feeValue
                                        break;
                                    case UserTransactionType.Withdrawal:
                                        if (userTransaction.ToAsset == "EUR")
                                        {
                                            transactions.Add(new Transaction(
                                            Wallet.WalletName,
                                            userTransaction.TransactionTime,
                                            TransactionType.BankWithdrawal,
                                            string.Empty, //assetStart
                                            userTransaction.FromAsset, //assetDestination
                                            null, //amountStart
                                            userTransaction.FromAmount, //amountDestination
                                            Wallet.WalletName, //locationStart
                                            "Bank", //locationDestination
                                            userTransaction.FromAmount, //value
                                            userTransaction.FeeAmount, //feeAmount
                                            userTransaction.FeeAsset, //feeAsset
                                            userTransaction.FeeAmount)); //feeValue
                                        }
                                        else
                                        {
                                            decimal feeValue;
                                            if (userTransaction.FeeAsset == "EUR")
                                            {
                                                feeValue = userTransaction.FeeAmount;
                                            }
                                            else
                                            {
                                                //Check, if there is an ExchangeRate already for the specific time and asset
                                                feeValue = userTransaction.FeeAmount * GetEURValue(userTransaction.FeeAsset, userTransaction.TransactionTime);
                                            }

                                            transactions.Add(new Transaction(
                                            Wallet.WalletName,
                                            userTransaction.TransactionTime,
                                            TransactionType.Transfer,
                                            userTransaction.FromAsset, //assetStart
                                            string.Empty, //assetDestination
                                            userTransaction.FromAmount, //amountStart
                                            null, //amountDestination
                                            Wallet.WalletName, //locationStart
                                            string.Empty, //locationDestination
                                            null, //value
                                            userTransaction.FeeAmount, //feeAmount
                                            userTransaction.FeeAsset, //feeAsset
                                            feeValue)); //feeValue
                                        }
                                        break;
                                    case UserTransactionType.MarketTrade:
                                        if (userTransaction.FromAsset != null && userTransaction.ToAsset != null)
                                        {
                                            decimal value;
                                            decimal feeValue;
                                            if (userTransaction.FromAsset == "EUR")
                                            {
                                                value = userTransaction.FromAmount;
                                            }
                                            else if (userTransaction.ToAsset == "EUR")
                                            {
                                                value = userTransaction.ToAmount;
                                            }
                                            else
                                            {
                                                value = userTransaction.FromAmount * GetEURValue(userTransaction.FromAsset, userTransaction.TransactionTime);
                                            }
                                            if (userTransaction.FeeAsset == "EUR")
                                            {
                                                feeValue = userTransaction.FeeAmount;
                                            }
                                            else
                                            {
                                                feeValue = userTransaction.FeeAmount * GetEURValue(userTransaction.FeeAsset, userTransaction.TransactionTime);
                                            }

                                            transactions.Add(new Transaction(
                                                Wallet.WalletName,
                                                userTransaction.TransactionTime,
                                                userTransaction.IsBuyer ? TransactionType.Buy : TransactionType.Sell,
                                                userTransaction.FromAsset, //assetStart
                                                userTransaction.ToAsset, //assetDestination
                                                userTransaction.FromAmount, //amountStart
                                                userTransaction.ToAmount, //AmountDestination
                                                string.Empty, //locationStart
                                                string.Empty, //locationDestination
                                                value, //value
                                                userTransaction.FeeAmount, //feeAmount
                                                userTransaction.FeeAsset, //feeAsset
                                                feeValue)); //feeValue
                                        }
                                        //else//Bitstamp sometimes has this mini transactions that are below 0.0001 euro
                                        //{
                                        //    transactions.Add(new Transaction(Wallet.WalletName, userTransaction.TransactionTime, userTransaction.IsBuyer ? TransactionType.Buy : TransactionType.Sell, userTransaction.FromAsset ?? "EUR", userTransaction.ToAsset ?? "EUR", userTransaction.FromAmount, userTransaction.ToAmount, Wallet.WalletName, Wallet.WalletName, -1.0m, userTransaction.FeeAmount, userTransaction.FeeAsset));
                                        //}
                                        break;
                                    case UserTransactionType.StakingReward:
                                        decimal stakeValue = userTransaction.ToAmount * GetEURValue(userTransaction.ToAsset, userTransaction.TransactionTime);
                                        transactions.Add(new Transaction(
                                                Wallet.WalletName,
                                                userTransaction.TransactionTime,
                                                TransactionType.Distribution,
                                                string.Empty, //assetStart
                                                userTransaction.ToAsset, //assetDestination
                                                null, //amountStart
                                                userTransaction.ToAmount, //AmountDestination
                                                string.Empty, //locationStart
                                                Wallet.WalletName, //locationDestination
                                                stakeValue, //value
                                                null, //feeAmount
                                                string.Empty, //feeAsset
                                                null)); //feeValue
                                        break;
                                    case UserTransactionType.SubAccountTransfer:
                                    case UserTransactionType.CreditedWithStakedAssets:
                                    case UserTransactionType.SentAssetsToStaking:
                                    case UserTransactionType.ReferralReward:
                                    case UserTransactionType.InterAccountTransfer:
                                        CryptoCenter.InternalInstance.AddLog("BitstampAgent: No implementation for UserTransactionType " + userTransaction.Type.ToString(), Wallet.WalletName);
                                        break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    CryptoCenter.InternalInstance.AddLog(userTransactionsResult.Error.Message, Wallet.WalletName);
                }
            }
            while (userTransactions.Count() == 1000);
            #endregion

            //Pass to CryptoCenter
            transactions.Sort();
            CryptoCenter.InternalInstance.IsBusy = false;
            foreach (Transaction transaction in transactions)
            {
                CryptoCenter.InternalInstance.AddNewTransaction(transaction);
            }
#if DEBUG
            Console.WriteLine("Bitstamp Wallet initialized. Name: " + Wallet.WalletName);
#endif
        }

        private decimal GetEURValue(string asset, DateTime dateTime)
        {
            if (asset == "EUR")
            {
                return 1.0m;
            }
            //Check, if ExchangeRateRequest already exist
            if (!exchangeRates.Any(x => x.Asset == asset))
            {
                exchangeRates.Add(new ExchangeRateRequest(asset));
            }
            var request = exchangeRates.First(x => x.Asset == asset);
            //Check if ExchangeRateRequest has Rates for specific time
            if (!request.Rates.Any(x => dateTime > x.OpenTime && dateTime < x.CloseTime))
            {
                //Load data if necessary and cache it during runtime for later use
                var result = Client.Api.Public.GetOHLCDataAsync((asset.ToLower() + "eur"), 60, 1000, dateTime.CutMinutesAndSeconds()).Result;
                if (result.Success)
                {
                    foreach (BitstampKline kline in result.Data.Data.OHLC)
                    {
                        request.Rates.Add(new ExchangeRate(kline.Timestamp, kline.Timestamp.AddMilliseconds(59999), (kline.High + kline.Low + kline.Open + kline.Close) / 4));
                    }
                }
                else
                {
                    return 0.0m;
                }
            }
            //return median price
            return exchangeRates.First(x => x.Asset == asset).Rates.First(x => dateTime > x.OpenTime && dateTime < x.CloseTime).MedianRate;
        }
    }
}