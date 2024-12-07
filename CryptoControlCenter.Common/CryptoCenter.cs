using Binance.Net.Clients;
using Binance.Net.Objects.Models.Spot;
using Bitstamp.Net;
using Bitstamp.Net.Objects;
using CryptoControlCenter.Common.Database;
using CryptoControlCenter.Common.Enums;
using CryptoControlCenter.Common.Helper;
using CryptoControlCenter.Common.Models;
using CryptoControlCenter.Common.Models.Interfaces;
using CryptoExchange.Net.Authentication;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using CryptoControlCenter.Common.Resources;

namespace CryptoControlCenter.Common
{
    /// <summary>
    /// This singleton class is the main public class of the library, providing access to the necessary data.
    /// Use CryptoCenter.Instance to get access to objects and methods.
    /// </summary>
    public sealed class CryptoCenter : AbstractPropertyChanged, ICryptoCenter
    {
        private System.Timers.Timer priceTimer;
        private bool isBusy;
        private bool containsMissingValues;
        private bool pricesChanged;
        internal static bool isInitialized = false;
        internal static bool initialzationRunning = false;
        internal static Currency currency = Currency.Euro;
        public static int DatabaseVersion { get; private set; }

        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                isBusy = value;
                OnPropertyChanged();
            }
        }

        public bool ContainsMissingValues
        {
            get { return containsMissingValues; }
            set
            {
                containsMissingValues = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Set Culture of Library
        /// </summary>
        /// <param name="languageCode">ISO Language Code</param>
        public static void SetLanguage(string languageCode)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(languageCode);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(languageCode);
        }
        /// <summary>
        /// Set Currency
        /// </summary>
        /// <param name="_currency"></param>
        public static void SetCurrency(Currency _currency)
        {
            if (_currency == Currency.USDollar)
            {
                throw new NotImplementedException("Only supports EUR at the moment.");
            }
            currency = _currency;
            if (isInitialized)
            {
                InternalInstance.RefreshBalanceValues();
            }
        }
        /// <summary>
        /// Initializes the Library. This method gets called automatically if you try to get an uninitialized Instance. However you can call this during your AppStartup to speedup loadup time.
        /// </summary>
        public static void Initialize()
        {
            if (!initialzationRunning && !isInitialized)
            {
                DatabaseVersion = SQLiteDatabaseManager.Database.LibVersionNumber;
                InternalInstance.IsBusy = true;
                initialzationRunning = true;
                List<Task> taskList = new List<Task>();
                ConcurrentBag<Exception> exceptionBag = new ConcurrentBag<Exception>();
                taskList.Add(Task.Run(() =>
                {
                    try
                    {
                        InternalInstance.BitstampSymbols.Clear();
                        InternalInstance.BinanceSymbols.Clear();
                        InternalInstance.ActualizeSymbols();
                    }
                    catch (Exception ex)
                    {
                        exceptionBag.Add(ex);
                    }
                }));
                taskList.Add(Task.Run(() =>
                {
                    //I've decided to not use Binance WebSocket but rather do a refresh of Prices every 5 minutes as
                    //Binance Websocket only offers updates per symbol in a specific interval (too many subscriptions would be necessary) or 
                    //updates, whenever a symbol price changes (too many updates because no possibility of a specific interval)
                    InternalInstance.FetchPrices();
                    InternalInstance.priceTimer.Elapsed += (source, e) =>
                    {
                        InternalInstance.FetchPrices();
                    };
                    InternalInstance.priceTimer.Interval = 300000;
                    InternalInstance.priceTimer.Start();
                }));
                #region Load existing entries from database
                taskList.Add(Task.Run(async () =>
                {
                    try
                    {
                        InternalInstance.ExchangeWallets.Clear();
                        (await SQLiteDatabaseManager.Database.Table<ExchangeWallet>().ToListAsync()).ForEach(InternalInstance.ExchangeWallets.Add);
                    }
                    catch (Exception ex)
                    {
                        exceptionBag.Add(ex);
                    }
                }));
                taskList.Add(Task.Run(async () =>
                {
                    try
                    {
                        InternalInstance.Transactions.Clear();
                        var list = await SQLiteDatabaseManager.Database.Table<Transaction>().ToListAsync();
                        list.Sort();
                        if (list != null) { list.ForEach(InternalInstance.Transactions.Add); }
                    }
                    catch (Exception ex)
                    {
                        exceptionBag.Add(ex);
                    }
                }));

                Task.WhenAll(taskList).Wait();
                foreach (Exception ex in exceptionBag)
                {
                    InternalInstance.AddLog(ex.Message, Strings.Initialization);
                    //TODO
                }
                #endregion
                isInitialized = true;
                initialzationRunning = false;
                InternalInstance.AddLog(Strings.InitCompleted, Strings.Initialization, false);
                InternalInstance.IsBusy = false;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        private CryptoCenter()
        {
            #region Instanciation
            pricesChanged = true;
            priceTimer = new System.Timers.Timer();
            taskQueue = new ObservableTaskQueue();
            taskQueue.StartWorking += () =>
            {
                InternalInstance.IsBusy = true;
                QueueRunning = true;
            };
            taskQueue.StopWorking += () =>
            {
                InternalInstance.IsBusy = false;
                QueueRunning = false;
            };
            Logs = new ObservableCollection<ILogEntry>();
            BinanceSymbols = new List<BinanceSymbol>();
            BitstampSymbols = new List<BitstampTradingPairInfo>();
            BinancePrices = new ObservableCollection<BinancePrice>();
            BitstampPrices = new ObservableCollection<BitstampTicker>();
            ExchangeWallets = new ObservableCollection<IExchangeWalletViewer>();
            Transactions = new ObservableCollection<Transaction>();
            #endregion
        }

        /// <summary>
        /// Lazy instanciation
        /// </summary>
        private static readonly Lazy<CryptoCenter> lazy = new Lazy<CryptoCenter>(() => new CryptoCenter());
        /// <summary>
        /// Get Instance
        /// </summary>
        public static ICryptoCenter Instance
        {
            get
            {
                if (!isInitialized)
                {
                    if (!initialzationRunning)
                    {
                        Task.Run(Initialize);
                    }
                    Task.Delay(100);
                    while (initialzationRunning)
                    {
                        Task.Delay(1000);
                    }
                }
                return lazy.Value;
            }
        }
        /// <summary>
        /// Get Internal Instance - this is used by other classes inside this assembly, if they need to access members of this object.
        /// </summary>
        internal static CryptoCenter InternalInstance
        {
            get
            {
                return lazy.Value;
            }
        }
        /// <inheritdoc/>
        public ObservableCollection<ILogEntry> Logs { get; private set; }
        /// <inheritdoc/>
        public ObservableCollection<IExchangeWalletViewer> ExchangeWallets { get; private set; }
        /// <inheritdoc/>
        public ObservableCollection<Transaction> Transactions { get; private set; }

        private ObservableCollection<HodledAsset> currentAssets;
        /// <inheritdoc/>
        public List<IBalanceViewer> CurrentAssets
        {
            get
            {
                if (currentAssets == null || InternalInstance.ContainsMissingValues)
                {
                    currentAssets = new ObservableCollection<HodledAsset>();
                    InternalInstance.ContainsMissingValues = false;
                    SortedSet<HodledAsset> hodledAssets = new SortedSet<HodledAsset>();
                    List<Transaction> transactions = SQLiteDatabaseManager.Database.Table<Transaction>().ToListAsync().Result;
                    if (transactions.Count != 0)
                    {
                        transactions.Sort();
                        ConcurrentBag<bool> invalid = new ConcurrentBag<bool>();
                        Parallel.ForEach(transactions, t =>
                        {
                            invalid.Add(t.Validate());
                        });
                        if (invalid.Contains(false))
                        {
                            InternalInstance.ContainsMissingValues = true;
                        }
                        Dictionary<string, FinancialStatementHelper> fshelper = new Dictionary<string, FinancialStatementHelper>();
                        List<string> wallets = new List<string>();
                        wallets.AddRange(transactions.Select(x => x.LocationStart));
                        wallets.AddRange(transactions.Select(x => x.LocationDestination));
                        foreach (string walletname in wallets.Distinct())
                        {
                            if (!string.IsNullOrWhiteSpace(walletname) && walletname != "Bank")
                            {
                                fshelper.Add(walletname, new FinancialStatementHelper());
                            }
                        }
                        try
                        {
                            foreach (Transaction transaction in transactions)
                            {
                                transaction.Process(ref fshelper, ref hodledAssets);
                            }
                        }
                        catch
                        {
                            InternalInstance.ContainsMissingValues = true;
                        }
                        if (!InternalInstance.ContainsMissingValues)
                        {
                        
                            foreach (HodledAsset asset in hodledAssets)
                            {
                                if (!asset.removed)
                                {

                                    currentAssets.Add(asset);
                                }
                            }
                        }
                    }
                }
                return currentAssets.ToList<IBalanceViewer>();
            }
        }
        /// <inheritdoc/>
        public List<string> WalletNames
        {
            get
            {
                List<string> output = new List<string>();
                foreach (IExchangeWalletViewer wallet in ExchangeWallets)
                {
                    output.Add(wallet.WalletName);
                }
                return output;
            }
        }
        #region Queue
        /// <inheritdoc/>
        public bool QueueRunning
        {
            get
            {
                return queueRunning;
            }
            private set
            {
                queueRunning = value;
                OnPropertyChanged();
            }
        }
        private bool queueRunning;

        /// <summary>
        /// Queue for all incoming Tasks
        /// </summary>
        private ObservableTaskQueue taskQueue { get; }
        #endregion

        /// <summary>
        /// List of available binance symbols
        /// </summary>
        internal List<BinanceSymbol> BinanceSymbols { get; set; }
        /// <summary>
        /// List of available bitstamp symbols
        /// </summary>
        internal List<BitstampTradingPairInfo> BitstampSymbols { get; set; }
        /// <summary>
        /// Collection of all current symbol prices on Binance
        /// </summary>
        internal ObservableCollection<BinancePrice> BinancePrices { get; set; }
        /// <summary>
        /// Collection of all current symbol prices on Bitstamp
        /// </summary>
        internal ObservableCollection<BitstampTicker> BitstampPrices { get; set; }


        /// <summary>
        /// This should be called upon closing/shutting down the app
        /// </summary>
        public static void AppClosureHandling()
        {
            if (isInitialized)
            {
                //TODO Add all services here, which need special shutdown handling
                InternalInstance.priceTimer.Stop();
                InternalInstance.taskQueue.Enqueue(async () => await SQLiteDatabaseManager.AppClosureHandling());

                Task.Delay(3000).Wait();
                int retries = 0;
                while (InternalInstance.QueueRunning && retries < 5)
                {
                    retries++;
                    InternalInstance.AddLog(Strings.ShutdownP1 + retries + Strings.ShutdownP2, Strings.Shutdown, false);
                    Task.Delay(5000).Wait();
                }
            }
        }
        /// <summary>
        /// Actualize the binance symbols list
        /// </summary>
        private void ActualizeSymbols()
        {
            bool successedBinance = false;
            bool successedBitstamp = false;

            try
            {
                var dd = SQLiteDatabaseManager.Database.GetAsync<CachedData>(1).Result;
#if DEBUG
                if (dd.LastSync.IsWithinTimeSpan(DateTime.Now, new TimeSpan(-15, 0, 0, 0)))
#else
                    if (dd.LastSync.IsWithinTimeSpan(DateTime.Now, new TimeSpan(-1, 0, 0, 0)))
#endif
                {
                    BitstampSymbols = JsonConvert.DeserializeObject<List<BitstampTradingPairInfo>>(dd.SerializedBitstamp);
                    BinanceSymbols = JsonConvert.DeserializeObject<BinanceExchangeInfo>(dd.SerializedBinance).Symbols.ToList();
                    InternalInstance.AddLog(Strings.CachedSymbols, Strings.Initialization);
                    return;
                }
            }
            catch
            {
                InternalInstance.AddLog(Strings.NoCachedSymbols, Strings.Initialization);
            }
            CachedData cachedData = new CachedData()
            {
                Id = 1,
                LastSync = DateTime.Now
            };
            var cancelSource = new CancellationTokenSource();
            List<Task> taskList = new List<Task>();
            taskList.Add(Task.Run(async () =>
            {
                InternalInstance.AddLog(Strings.BinanceSymbols, Strings.Symbols);
                BinanceRestClient.SetDefaultOptions(options =>
                {
                    options.OutputOriginalData = true;
                });
                var clientBinance = new BinanceRestClient();
                var resultBinance = await clientBinance.SpotApi.ExchangeData.GetExchangeInfoAsync(cancelSource.Token);
                if (resultBinance.Success)
                {
                    BinanceSymbols = resultBinance.Data.Symbols.ToList();
                    successedBinance = true;
                    cachedData.SerializedBinance = resultBinance.OriginalData;
                }
                InternalInstance.AddLog(Strings.BinanceSymbolsEnd, Strings.Symbols);
            }));
            taskList.Add(Task.Run(async () =>
            {
                InternalInstance.AddLog(Strings.BitstampSymbols, Strings.Symbols);
                BitstampClient.SetDefaultOptions(options =>
                {
                    options.OutputOriginalData = true;
                });
                var clientBitstamp = new BitstampClient();
                var resultBitstamp = await clientBitstamp.Api.Public.GetTradingPairInfo(cancelSource.Token);
                if (resultBitstamp.Success)
                {
                    BitstampSymbols = resultBitstamp.Data.ToList();
                    successedBitstamp = true;
                    cachedData.SerializedBitstamp = resultBitstamp.OriginalData;
                }
                InternalInstance.AddLog(Strings.BitstampSymbolsEnd, Strings.Symbols);
            }));
            Task.Run(async () =>
            {
                await Task.Delay(30000);
                if (successedBinance == false || successedBitstamp == false)
                {
                    cancelSource.Cancel();
                    InternalInstance.AddLog(Strings.SymbolsFailed, Strings.Symbols);
                    throw new InvalidOperationException("Loading symbols failed");
                }
            });
            Task.WhenAll(taskList).Wait();
            if (successedBinance && successedBitstamp)
            {
                int i = SQLiteDatabaseManager.Database.InsertOrReplaceAsync(cachedData).Result;
                InternalInstance.AddLog(Strings.CacheUpdated, Strings.Symbols);
            }
        }
        /// <summary>
        /// Fetches current prices of Binance and Bitstamp
        /// </summary>
        private async void FetchPrices()
        {
            List<Task> taskList = new List<Task>();
            taskList.Add(Task.Run(async () =>
            {
                var clientBitstamp = new BitstampClient();
                var resultBitstamp = await clientBitstamp.Api.Public.GetAllTickersAsync();
                if (resultBitstamp.Success)
                {
                    BitstampPrices.Clear();
                    foreach (BitstampTicker price in resultBitstamp.Data)
                    {
                        BitstampPrices.Add(price);
                    }
                }
                else { InternalInstance.AddLog(resultBitstamp.Error.Message, Strings.FetchPrices); }
            }));
            taskList.Add(Task.Run(async () =>
            {
                var clientBinance = new BinanceRestClient();
                var resultBinance = await clientBinance.SpotApi.ExchangeData.GetPricesAsync();
                if (resultBinance.Success)
                {
                    BinancePrices.Clear();
                    foreach (BinancePrice price in resultBinance.Data)
                    {
                        BinancePrices.Add(price);
                    }
                }
                else
                {
                    InternalInstance.AddLog(resultBinance.Error.Message, Strings.FetchPrices);
                }
            }));
            await Task.WhenAll(taskList);
            pricesChanged = true;
        }
        /// <inheritdoc/>
        public void RefreshBalanceValues()
        {
            if (BinancePrices != null && BinancePrices.Count > 0 && BitstampPrices != null && BitstampPrices.Count > 0 && CurrentAssets != null && CurrentAssets.Count > 0 && pricesChanged)
            {
                decimal conversionRateBTCBinance = 1.0m;
                decimal conversionRateBTCBitstamp = 1.0m;
                switch (currency)
                {
                    case Currency.USDollar:
                        throw new NotImplementedException("Only supports EUR at the moment.");
                    //conversionRateBTCBinance = (double)BinancePrices.First(x => x.Symbol == "BTCUSDT").Price;
                    //conversionRateBTCBitstamp = (double)BitstampPrices.First(x => x.Pair == "BTC/USD").Last;
                    //break;
                    case Currency.Euro:
                        conversionRateBTCBinance = BinancePrices.First(x => x.Symbol == "BTCEUR").Price;
                        conversionRateBTCBitstamp = BitstampPrices.First(x => x.Pair == "BTC/EUR").Last;
                        break;
                }
                Parallel.ForEach(currentAssets, balance =>
                {
                    if(balance.Asset == "EUR")
                    {
                        balance.CurrentValue = balance.CurrentAmount;
                    }
                    try
                    {
                        decimal conversionRate = 1.0m;
                        if (!string.IsNullOrWhiteSpace(balance.Wallet))
                        {
                            switch (InternalInstance.ExchangeWallets.First(x => x.WalletName == balance.Wallet).Exchange)
                            {
                                case Exchange.Binance:
                                    if (balance.Asset == "BTC")
                                    {
                                        balance.CurrentValue = balance.CurrentAmount * conversionRateBTCBinance;
                                    }
                                    else
                                    {
                                        conversionRate = BinancePrices.First(x => x.Symbol == balance.Asset + "BTC").Price;
                                        balance.CurrentValue = balance.CurrentAmount * conversionRate * conversionRateBTCBinance;
                                    }
                                    break;
                                case Exchange.Bitstamp:
                                    try
                                    {
                                        conversionRate = BitstampPrices.First(x => x.Pair == balance.Asset.ToUpper() + "/EUR").Last;
                                        balance.CurrentValue = balance.CurrentAmount * conversionRate;
                                    }
                                    catch
                                    {
                                        conversionRate = BitstampPrices.First(x => x.Pair == balance.Asset.ToUpper() + "/BTC").Last;
                                        balance.CurrentValue = balance.CurrentAmount * conversionRate * conversionRateBTCBitstamp;
                                    }
                                    break;
                                default:
                                    //TODO: Marker
                                    break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
#if DEBUG
                        Console.WriteLine(balance.Wallet + ": " + balance.Asset);
#endif
                        //TODO
                    };
                });
                pricesChanged = false;
                OnPropertyChanged("CurrentAssets");
            }
        }

        /// <inheritdoc/>
        public void SynchronizeWallets()
        {
            InternalInstance.IsBusy = true;

            Parallel.ForEach(InternalInstance.ExchangeWallets, async (wallet) =>
            {
                if (wallet.Exchange == Exchange.Bitstamp)
                {
                    DateTime lastTransaction = Transactions.Last(x => x.Wallet == wallet.WalletName).TransactionTime;
                    lastTransaction.AddSeconds(1); //so the last transaction will not be included
                    await taskQueue.Enqueue(async () => await wallet.SynchronizeWallet(lastTransaction));
                }
            });
            InternalInstance.IsBusy = false;
        }

        /// <inheritdoc/>
        public async void CreateWallet(string walletName, Exchange exchange)
        {
            if (string.IsNullOrWhiteSpace(walletName))
            {
                throw new ArgumentException("WalletName can't be empty or whitespace.");
            }
            else
            {
                InternalInstance.IsBusy = true;
                var resultW = await SQLiteDatabaseManager.Database.FindAsync<ExchangeWallet>(x => x.WalletName == walletName);
                if (resultW == null)
                {
                    var secureCredential = new SecuredCredentials(walletName, "dummy");
                    await SQLiteDatabaseManager.Database.InsertAsync(secureCredential);
                    int secureid = SQLiteDatabaseManager.Database.GetAsync<SecuredCredentials>(x => x.Key == walletName).Result.ID;
                    IExchangeWalletViewer wallet = new ExchangeWallet(walletName, exchange, secureid);
                    await SQLiteDatabaseManager.Database.InsertAsync(wallet);
                    ExchangeWallets.Add(wallet);
                    InternalInstance.IsBusy = false;
                }
                else
                {
                    InternalInstance.IsBusy = false;
                    throw new InvalidOperationException("Wallet Name already exists.");
                }
            }
        }
        /// <inheritdoc/>
        public async void CreateWallet(string walletName, Exchange exchange, string csvFilePathTransactions, string csvFilePathWithdrawalDeposits, string csvFilePathDistribution, DateTime startingPoint)
        {
            if (string.IsNullOrWhiteSpace(walletName) || string.IsNullOrWhiteSpace(csvFilePathTransactions) || string.IsNullOrWhiteSpace(csvFilePathWithdrawalDeposits) || string.IsNullOrWhiteSpace(csvFilePathDistribution))
            {
                throw new ArgumentException("File paths or wallet name can't be empty or whitespace.");
            }
            else
            {
                InternalInstance.IsBusy = true;
                var resultW = await SQLiteDatabaseManager.Database.FindAsync<ExchangeWallet>(x => x.WalletName == walletName);
                if (resultW == null)
                {
                    string keyname = walletName + "dummy";
                    var secureCredential = new SecuredCredentials(keyname, "dummy");
                    await SQLiteDatabaseManager.Database.InsertAsync(secureCredential);
                    int secureid = SQLiteDatabaseManager.Database.GetAsync<SecuredCredentials>(x => x.Key == keyname).Result.ID;
                    IExchangeWalletViewer wallet = new ExchangeWallet(walletName, exchange, secureid);
                    await SQLiteDatabaseManager.Database.InsertAsync(wallet);
                    ExchangeWallets.Add(wallet);
                    wallet.ImportFromCSV(csvFilePathTransactions, csvFilePathWithdrawalDeposits, csvFilePathDistribution, startingPoint);
                    InternalInstance.IsBusy = false;
                }
                else
                {
                    InternalInstance.IsBusy = false;
                    throw new InvalidOperationException("Wallet Name already exists.");
                }
            }
        }
        /// <inheritdoc/>
        public async void CreateWallet(string walletName, Exchange exchange, string exchangeApiKey, string exchangeApiSecret, DateTime startingPoint)
        {
            if (string.IsNullOrWhiteSpace(walletName) || string.IsNullOrWhiteSpace(exchangeApiKey) || string.IsNullOrWhiteSpace(exchangeApiSecret))
            {
                throw new ArgumentException("API Credentials or Wallet Name can't be empty or whitespace.");
            }
            else
            {
                InternalInstance.IsBusy = true;
                //check for proper priviliges
                switch (exchange)
                {
                    case Exchange.Binance:
                        using (var client = new BinanceRestClient())
                        {
                            try
                            {
                                client.SpotApi.SetApiCredentials(new ApiCredentials(exchangeApiKey, exchangeApiSecret));
                                var result = await client.SpotApi.Account.GetAPIKeyPermissionsAsync();
                                if (result.Success && result.Data.EnableReading)
                                {
                                    //everything ok
                                }
                                else
                                {
                                    InternalInstance.IsBusy = false;
                                    throw new InvalidOperationException("Wrong credentials or unsufficent permissions");
                                }
                            }
                            catch
                            {
                                InternalInstance.IsBusy = false;
                                throw new InvalidOperationException("Can't reach exchange. Please check connectivity.");
                            }
                        }
                        break;
                    case Exchange.Bitstamp:
                        using (var client = new BitstampClient())
                        {
                            try
                            {
                                client.Api.SetApiCredentials(new ApiCredentials(exchangeApiKey, exchangeApiSecret));
                                var result = await client.Api.Private.GetUserTransactionsAsync(limit: 10);
                                if (!result.Success)
                                {
                                    //Bitstamp misses a comparable method as binance has, so instead, we try to read user transaction data
                                    InternalInstance.IsBusy = false;
                                    throw new InvalidOperationException(result.Error.Message);
                                }
                            }
                            catch
                            {
                                InternalInstance.IsBusy = false;
                                throw new InvalidOperationException("Can't reach exchange. Please check connectivity.");
                            }
                        }
                        break;
                    default:
                        InternalInstance.IsBusy = false;
                        throw new NotImplementedException("Unsupported Exchange choosen.");
                }

                //insert to DB
                var resultSC = await SQLiteDatabaseManager.Database.FindAsync<SecuredCredentials>(x => x.Key == exchangeApiKey);
                var resultW = await SQLiteDatabaseManager.Database.FindAsync<ExchangeWallet>(x => x.WalletName == walletName);
                if (resultSC == null && resultW == null)
                {
                    var secureCredential = new SecuredCredentials(exchangeApiKey, exchangeApiSecret);
                    await SQLiteDatabaseManager.Database.InsertAsync(secureCredential);
                    int secureid = SQLiteDatabaseManager.Database.GetAsync<SecuredCredentials>(x => x.Key == exchangeApiKey).Result.ID;
                    IExchangeWalletViewer wallet = new ExchangeWallet(walletName, exchange, secureid);
                    await SQLiteDatabaseManager.Database.InsertAsync(wallet);
                    ExchangeWallets.Add(wallet);
                    await taskQueue.Enqueue(async () => await wallet.SynchronizeWallet(startingPoint));
                    InternalInstance.IsBusy = false;
                }
                else
                {
                    InternalInstance.IsBusy = false;
                    throw new InvalidOperationException("Wallet Name or API Key already exists.");
                }
            }
        }
        /// <inheritdoc/>
        public void RemoveWallet(IExchangeWalletViewer wallet)
        {
            taskQueue.Enqueue(async () => await removeWalletTask(wallet));
        }
        /// <summary>
        /// Task to remove a Wallet.
        /// </summary>
        /// <param name="wallet">Wallet to be deleted</param>
        private async Task removeWalletTask(IExchangeWalletViewer wallet)
        {
            try
            {
                await SQLiteDatabaseManager.Database.DeleteAsync<SecuredCredentials>(wallet.SecureCredentialID);
                await SQLiteDatabaseManager.Database.DeleteAsync(wallet);
                List<Transaction> transactions = await SQLiteDatabaseManager.Database.Table<Transaction>().Where(x => x.LocationStart == wallet.WalletName).ToListAsync();
                List<Transaction> transactions2 = await SQLiteDatabaseManager.Database.Table<Transaction>().Where(x => (x.LocationStart == "Bank" || x.LocationStart == string.Empty) && x.LocationDestination == wallet.WalletName).ToListAsync();
                Parallel.ForEach(transactions, async (transaction) =>
                {
                    await SQLiteDatabaseManager.Database.DeleteAsync(transaction);
                });
                Parallel.ForEach(transactions2, async (transaction) =>
                {
                    await SQLiteDatabaseManager.Database.DeleteAsync(transaction);
                });
                ExchangeWallets.Remove(wallet);
            }
            catch (InvalidOperationException)
            {
                throw new InvalidOperationException("Wallet not found: " + wallet.WalletName);
            }
        }
        /// <summary>
        /// Logs an error to database and local list
        /// </summary>
        internal void AddLog(string message, string? trigger = null, bool permanent = true)
        {
            taskQueue.Enqueue(async () => await LogTask(message, trigger));
        }
        /// <summary>
        /// New Log Task - called by public InternalInstance.AddLog-Method
        /// </summary>
        private async Task LogTask(string message, string? trigger = null, bool permanent = true)
        {
            LogEntry entry = new LogEntry(message, trigger);
#if DEBUG
            Console.WriteLine((trigger == null ? string.Empty : (trigger + ": ")) + message);
#else
            if(permanent)
            {
                await SQLiteDatabaseManager.Database.InsertAsync(entry);
            }
            Logs.Add(entry);
#endif
        }
        /// <inheritdoc/>
        public void AddNewTransaction(Transaction transaction)
        {
            taskQueue.Enqueue(async () => await NewTransactionTask(transaction));
        }
        /// <inheritdoc/>
        public void DeleteTransaction(Transaction transaction)
        {
            if (transaction != null)
            {
                taskQueue.Enqueue(async () => await DeleteTransactionTask(transaction));
            }
        }
        /// <summary>
        /// Delete Transaction Task - called by the public DeleteTransaction-Method
        /// </summary>
        private async Task DeleteTransactionTask(Transaction transaction)
        {
            await SQLiteDatabaseManager.Database.DeleteAsync(transaction);
            Transactions.Remove(transaction);
#if DEBUG
            InternalInstance.AddLog(Strings.TransactionDeleted + transaction.GetTradingPair(), Strings.TransactionTask);
#endif
        }


        /// <summary>
        /// New Transaction Task - called by the public AddNewTransaction-Method
        /// </summary>
        private async Task NewTransactionTask(Transaction transaction)
        {
            Transactions.Add(transaction);
            await SQLiteDatabaseManager.Database.InsertAsync(transaction);
#if DEBUG
            switch (transaction.TransactionType)
            {
                case Enums.TransactionType.Buy:
                case Enums.TransactionType.Sell:
                    InternalInstance.AddLog(Strings.NewTransaction + transaction.GetTradingPair(), Strings.TransactionTask);
                    break;
                default:
                    InternalInstance.AddLog(Strings.NewTransaction + transaction.TransactionType.ToString() + " " + transaction.AssetStart, Strings.TransactionTask);
                    break;
            }
#endif
        }
    }
}