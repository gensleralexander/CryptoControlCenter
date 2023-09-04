using Binance.Net.Clients;
using Binance.Net.Enums;
using Binance.Net.Interfaces.Clients;
using Binance.Net.Objects;
using Binance.Net.Objects.Models.Spot;
using CryptoControlCenter.Common.Database;
using CryptoControlCenter.Common.Models;
using CryptoControlCenter.Common.Models.Interfaces;
using CryptoExchange.Net.Authentication;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CryptoControlCenter.Common.DataProvider
{
    /// <summary>
    /// Data Provider for Binance Wallets
    /// </summary>
    public class BinanceDataProvider : IDataProvider
    {
        private IBinanceRestClient Client;
        /// <inheritdoc/>
        public IExchangeWalletViewer Wallet { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public BinanceDataProvider(IExchangeWalletViewer wallet)
        {
            Wallet = wallet;
            SecuredCredentials credentials = SQLiteDatabaseManager.Database.FindAsync<SecuredCredentials>(x => x.ID == Wallet.SecureCredentialID).Result;
            if (credentials != null)
            {
                BinanceRestClient.SetDefaultOptions(options =>
                {
                    options.ApiCredentials = new ApiCredentials(credentials.Key, credentials.GetDecryptedSecret());
                });
            }
            Client = new BinanceRestClient();

        }
        /// <inheritdoc/>
        public void ImportFromCSV(string csvFilePathTransactions, string csvFilePathWithdrawalDeposits, string csvFilePathDistribution)
        {
            int id = 100000000; //give new transaction an ID, because sometimes, transactions are identical and would falsly get dropped
            var culture = new CultureInfo("en-US");
            SortedSet<Transaction> transactions = new SortedSet<Transaction>();
            StreamReader reader;
            if (!string.IsNullOrWhiteSpace(csvFilePathTransactions))
            {
                reader = new StreamReader(Path.GetFullPath(csvFilePathTransactions));
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    csv.Read();
                    csv.ReadHeader();
                    while (csv.Read())
                    {
                        BinanceSymbol symbol = CryptoCenter.InternalInstance.BinanceSymbols.First(x => x.Name == csv.GetField("Symbol"));
                        bool isBuy = (csv.GetField("Side") == "BUY");
                        transactions.Add(new Transaction(
                            Wallet.WalletName,
                            DateTime.Parse(csv.GetField("DateUTC")),
                            isBuy ? Enums.TransactionType.Buy : Enums.TransactionType.Sell,
                            isBuy ? symbol.QuoteAsset : symbol.BaseAsset,
                            isBuy ? symbol.BaseAsset : symbol.QuoteAsset,
                            isBuy ? decimal.Parse(Regex.Replace(csv.GetField("Quote"), "[^0-9.]", string.Empty), culture) : decimal.Parse(Regex.Replace(csv.GetField("Base"), "[^0-9.]", string.Empty), culture),
                            isBuy ? decimal.Parse(Regex.Replace(csv.GetField("Base"), "[^0-9.]", string.Empty), culture) : decimal.Parse(Regex.Replace(csv.GetField("Quote"), "[^0-9.]", string.Empty), culture),
                            Wallet.WalletName,
                            Wallet.WalletName,
                            -1.0m,
                            decimal.Parse(Regex.Replace(csv.GetField("Fee"), "[^0-9.]", string.Empty), culture),
                            Regex.Replace(csv.GetField("Fee"), @"[\d-.]", string.Empty))
                        {
                            TransactionID = id
                        });
                        id++;
                    }
                }
            }
            if (!string.IsNullOrWhiteSpace(csvFilePathWithdrawalDeposits))
            {
                reader = new StreamReader(Path.GetFullPath(csvFilePathWithdrawalDeposits));
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    csv.Read();
                    csv.ReadHeader();
                    while (csv.Read())
                    {
                        bool isDeposit = (csv.GetField("Type") == "Deposit");
                        transactions.Add(new Transaction(
                            Wallet.WalletName,
                            DateTime.Parse(csv.GetField("DateUTC")),
                            Enums.TransactionType.Transfer,
                            csv.GetField("Coin"),
                            csv.GetField("Coin"),
                            decimal.Parse(csv.GetField("Amount"), culture),
                            decimal.Parse(csv.GetField("Amount"), culture),
                            isDeposit ? string.Empty : Wallet.WalletName,
                            isDeposit ? Wallet.WalletName : string.Empty,
                            -1.0m,
                            decimal.Parse(csv.GetField("TransactionFee"), culture),
                            csv.GetField("Coin"))
                        {
                            TransactionID = id
                        });
                        id++;
                    }
                }
            }
            if (!string.IsNullOrWhiteSpace(csvFilePathDistribution))
            {
                reader = new StreamReader(Path.GetFullPath(csvFilePathDistribution));
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    csv.Read();
                    csv.ReadHeader();
                    while (csv.Read())
                    {
                        if (csv.GetField("Operation") == "Distribution")
                        {
                            transactions.Add(new Transaction(
                                Wallet.WalletName,
                                DateTime.Parse(csv.GetField("DateUTC")),
                                Enums.TransactionType.Distribution,
                                csv.GetField("Coin"),
                                csv.GetField("Coin"),
                                decimal.Parse(csv.GetField("Change"), culture),
                                decimal.Parse(csv.GetField("Change"), culture),
                                Wallet.WalletName,
                                Wallet.WalletName,
                                -1.0m,
                                0.0m,
                                string.Empty)
                            {
                                TransactionID = id
                            });
                            id++;
                        }
                    }
                }
            }
            foreach (Transaction t in transactions)
            {
                CryptoCenter.InternalInstance.AddNewTransaction(t);
            }
        }

        /// <inheritdoc />
        [Obsolete("This method is currently not reliable in 'getting all transactions', as the Binance API requires a specific list of trading pairs for which you want to query transactions. For now, use CSV-Import, until this method is fixed.")]
        public async Task SynchronizeWallet(DateTime? startingPoint = null)
        {
            //Delays in this method are used to prevent API abuse.
#if DEBUG
            Console.WriteLine("Binance Provider requested synchronization. Wallet Name: " + Wallet.WalletName);
#endif
            DateTime startTime;
            DateTime endTime;
            DateTime businessStartBinance = new DateTime(2017, 7, 1);
            if (startingPoint == null || startingPoint < businessStartBinance)
            {
                startTime = businessStartBinance;
            }
            else if (startingPoint > DateTime.UtcNow)
            {
                throw new ArgumentException("Parameter startingPoint cannot be in the future.");
            }
            else
            {
                startTime = (DateTime)startingPoint;
            }
            if (DateTime.UtcNow - startTime < TimeSpan.FromDays(90))
            {
                endTime = DateTime.UtcNow;
            }
            else
            {
                endTime = startTime.AddDays(90);
            }
            HashSet<BinanceSymbol> querySet = new HashSet<BinanceSymbol>();
            List<string> queryHelper = new List<string>();
            List<Transaction> transactions = new List<Transaction>();

            #region DustLog
            var dustResult = await Client.SpotApi.Account.GetDustLogAsync(startTime, DateTime.UtcNow);
            if (dustResult.Success)
            {
                foreach (BinanceDustLog dust in dustResult.Data.UserAssetDribblets)
                {
                    foreach (BinanceDustLogDetails detail in dust.Logs)
                    {
                        queryHelper.Add(detail.FromAsset);
                        if (detail.OperateTime > startTime)
                        {
                            transactions.Add(new Transaction(Wallet.WalletName, detail.OperateTime, Enums.TransactionType.Dust, detail.FromAsset, "BNB", detail.Quantity, detail.TransferredQuantity, Wallet.WalletName, Wallet.WalletName, -1.0m, detail.ServiceChargeQuantity, "BNB"));
                        }
                    }
                }
            }
            else
            {
                //TODO Error Handling
                CryptoCenter.InternalInstance.AddLog(dustResult.Error.Message, Wallet.WalletName);
                throw new Exception(dustResult.Error.Message);
            }
            #endregion

            #region Distribution
            var distributionResult = await Client.SpotApi.Account.GetAssetDividendRecordsAsync(null, startTime, DateTime.UtcNow, 500, null, default);
            if (distributionResult.Success)
            {
                foreach (BinanceDividendRecord distribution in distributionResult.Data.Rows)
                {
                    queryHelper.Add(distribution.Asset);
                    transactions.Add(new Transaction(Wallet.WalletName, distribution.Timestamp, Enums.TransactionType.Distribution, distribution.Asset, distribution.Asset, distribution.Quantity, distribution.Quantity, Wallet.WalletName, Wallet.WalletName, -1.0m, 0.0m, string.Empty));
                }
                while (distributionResult.Data.Rows.Count() == 500)
                {
                    await Task.Delay(1000);
                    DateTime lastTimestamp = distributionResult.Data.Rows.Last().Timestamp;
                    distributionResult = await Client.SpotApi.Account.GetAssetDividendRecordsAsync(null, lastTimestamp.AddMilliseconds(1), DateTime.UtcNow, 500, null, default);
                    if (distributionResult.Success)
                    {
                        foreach (BinanceDividendRecord distribution in distributionResult.Data.Rows)
                        {
                            queryHelper.Add(distribution.Asset);
                            transactions.Add(new Transaction(Wallet.WalletName, distribution.Timestamp, Enums.TransactionType.Distribution, distribution.Asset, distribution.Asset, distribution.Quantity, distribution.Quantity, Wallet.WalletName, Wallet.WalletName, -1.0m, 0.0m, string.Empty));
                        }
                    }
                    else
                    {
                        CryptoCenter.InternalInstance.AddLog(distributionResult.Error.Message, Wallet.WalletName);
                    }
                }
            }
            else
            {
                CryptoCenter.InternalInstance.AddLog(distributionResult.Error.Message, Wallet.WalletName);
            }
            #endregion

            #region Create QueryList for Trade History
            var balancesResult = await Client.SpotApi.Account.GetAccountInfoAsync();
            if (balancesResult.Success)
            {
                var balances = balancesResult.Data.Balances.Where(x => x.Total > 0.0m || queryHelper.Contains(x.Asset));
                foreach (BinanceBalance asset in balances)
                {
                    querySet.UnionWith(CryptoCenter.InternalInstance.BinanceSymbols.Where(x => x.BaseAsset == asset.Asset && balances.Any(y => y.Asset == x.QuoteAsset)));
                }
            }
            #endregion

            #region Withdrawal/Deposits
            DateTime startWD = startTime;
            DateTime endWD = endTime;
            //Begin While-Loop (only for Withdrawal and Deposits)
            int indicator = 2;
            while (indicator != 0)  // This is used instead of (endTime < now) because it needs one more iteration when endTime = now
            {

                #region DepositHistory
                var depositResult = await Client.SpotApi.Account.GetDepositHistoryAsync(null, DepositStatus.Success, startWD, endWD, null, 1000);
                if (depositResult.Success)
                {
                    foreach (BinanceDeposit deposit in depositResult.Data)
                    {
                        querySet.UnionWith(CryptoCenter.InternalInstance.BinanceSymbols.Where(x => x.BaseAsset == deposit.Asset));
                        transactions.Add(new Transaction(Wallet.WalletName, deposit.InsertTime, Enums.TransactionType.Transfer, deposit.Asset, deposit.Asset, deposit.Quantity, deposit.Quantity, string.Empty, Wallet.WalletName, -1.0m, 0.0m, string.Empty));
                    }
                }
                else
                {
                    //TODO Error Handling
                    CryptoCenter.InternalInstance.AddLog(depositResult.Error.Message, Wallet.WalletName);
                    throw new Exception(depositResult.Error.Message);
                }
                await Task.Delay(1000);
                #endregion
                #region WithdrawHistory
                var withdrawResult = await Client.SpotApi.Account.GetWithdrawalHistoryAsync(null, null, WithdrawalStatus.Completed, startWD, endWD);
                if (withdrawResult.Success)
                {
                    foreach (BinanceWithdrawal withdrawal in withdrawResult.Data)
                    {
                        querySet.UnionWith(CryptoCenter.InternalInstance.BinanceSymbols.Where(x => x.BaseAsset == withdrawal.Asset));
                        transactions.Add(new Transaction(Wallet.WalletName, withdrawal.ApplyTime, Enums.TransactionType.Transfer, withdrawal.Asset, withdrawal.Asset, withdrawal.Quantity, withdrawal.Quantity, Wallet.WalletName, string.Empty, -1.0m, withdrawal.TransactionFee, withdrawal.Asset));
                    }
                }
                else
                {
                    //TODO Error Handling
                    CryptoCenter.InternalInstance.AddLog(withdrawResult.Error.Message, Wallet.WalletName);
                    throw new Exception(withdrawResult.Error.Message);
                }
                await Task.Delay(1000);
                #endregion
                startWD = endWD.AddMilliseconds(1);
                if (DateTime.UtcNow - startWD < TimeSpan.FromDays(90))
                {
                    endWD = DateTime.UtcNow;
                }
                else
                {
                    endWD = startWD.AddDays(90);
                    indicator++;
                }
                indicator--;
            }
            #endregion

            #region TradeHistory
            foreach (BinanceSymbol symbol in querySet)
            {
                IEnumerable<BinanceTrade> tradeResultData = new List<BinanceTrade>();
                var tradeResult = await Client.SpotApi.Trading.GetUserTradesAsync(symbol.Name, null, startTime, null, 1000, null, null, default);
                if (tradeResult.Success)
                {
                    tradeResultData = tradeResult.Data;
                    foreach (BinanceTrade trade in tradeResultData)
                    {
                        if (trade.IsBuyer)
                        {
                            transactions.Add(new Transaction(Wallet.WalletName, trade.Timestamp, Enums.TransactionType.Buy, symbol.QuoteAsset, symbol.BaseAsset, trade.QuoteQuantity, trade.Quantity, Wallet.WalletName, Wallet.WalletName, -1.0m, trade.Fee, trade.FeeAsset));
                        }
                        else
                        {
                            transactions.Add(new Transaction(Wallet.WalletName, trade.Timestamp, Enums.TransactionType.Sell, symbol.BaseAsset, symbol.QuoteAsset, trade.Quantity, trade.QuoteQuantity, Wallet.WalletName, Wallet.WalletName, -1.0m, trade.Fee, trade.FeeAsset));
                        }
                    }
                    while (tradeResultData.Count() == 1000)
                    {
                        await Task.Delay(1000);
                        long lastID = tradeResultData.Last().Id;
                        tradeResult = await Client.SpotApi.Trading.GetUserTradesAsync(symbol.Name, null, null, null, 1000, lastID);
                        if (tradeResult.Success)
                        {
                            tradeResultData = tradeResult.Data;
                            foreach (BinanceTrade trade in tradeResultData.Skip(1)) //Skip first as it is a duplicate
                            {
                                if (trade.IsBuyer)
                                {
                                    transactions.Add(new Transaction(Wallet.WalletName, trade.Timestamp, Enums.TransactionType.Buy, symbol.QuoteAsset, symbol.BaseAsset, trade.QuoteQuantity, trade.Quantity, Wallet.WalletName, Wallet.WalletName, -1.0m, trade.Fee, trade.FeeAsset));
                                }
                                else
                                {
                                    transactions.Add(new Transaction(Wallet.WalletName, trade.Timestamp, Enums.TransactionType.Sell, symbol.BaseAsset, symbol.QuoteAsset, trade.Quantity, trade.QuoteQuantity, Wallet.WalletName, Wallet.WalletName, -1.0m, trade.Fee, trade.FeeAsset));
                                }
                            }
                        }
                        else
                        {
                            CryptoCenter.InternalInstance.AddLog(tradeResult.Error.Message, Wallet.WalletName);
                        }
                    }
                }
                else
                {
                    CryptoCenter.InternalInstance.AddLog(tradeResult.Error.Message, Wallet.WalletName);
                }
                await Task.Delay(1000);
            }
            #endregion



            //Pass to Center
            transactions.Sort();
            foreach (Transaction transaction in transactions)
            {
                CryptoCenter.InternalInstance.AddNewTransaction(transaction);
            }
#if DEBUG
            Console.WriteLine("Binance Wallet synchronized. Name: " + Wallet.WalletName);
#endif
        }
    }
}