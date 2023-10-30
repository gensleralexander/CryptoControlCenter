using Binance.Net.Clients;
using Binance.Net.Enums;
using Binance.Net.Interfaces;
using Binance.Net.Objects.Models.Spot;
using Bitstamp.Net;
using Bitstamp.Net.Interfaces;
using Bitstamp.Net.Objects;
using CryptoControlCenter.Common.Database;
using CryptoControlCenter.Common.Enums;
using CryptoControlCenter.Common.Helper;
using CryptoControlCenter.Common.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CryptoControlCenter.Common.Models.Interfaces;
using System.Threading;
using Newtonsoft.Json;
using System.Globalization;
using Binance.Net.Interfaces.Clients;
using CryptoExchange.Net.Authentication;

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
        internal static bool isInitialized = false;
        internal static bool initialzationRunning = false;
        internal static Currency currency = Currency.Euro;

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
        /// <param name="languageCode">ISO Language Code</param>
        public static void SetCurrency(Currency _currency)
        {
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
                //I've decided to not use Binance WebSocket but rather do a refresh of Balance Values and Binance Prices every 5 minutes as
                //Binance Websocket only offers updates per symbol in a specific interval (too many subscriptions would be necessary) or 
                //updates, whenever a symbol price changes (too many updates because no possibility of a specific interval)
                InternalInstance.FetchPrices();
                InternalInstance.priceTimer.Elapsed += (source, e) =>
                {
                    InternalInstance.FetchPrices();
                    InternalInstance.RefreshBalanceValues();
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
                InternalInstance.AddLog(ex.Message, Resources.Strings.Initialization);
                //TODO
            }
            #endregion
            InternalInstance.RefreshBalanceValues();
            isInitialized = true;
            initialzationRunning = false;
            InternalInstance.AddLog(Resources.Strings.InitCompleted, Resources.Strings.Initialization, false);
            InternalInstance.IsBusy = false;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        private CryptoCenter()
        {
            #region Instanciation
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

        private ObservableCollection<WalletBalance> currentAssets;
        /// <inheritdoc/>
        public List<IBalanceViewer> CurrentAssets
        {
            get
            {
                if (currentAssets == null)
                {
                    currentAssets = new ObservableCollection<WalletBalance>();
                    SortedSet<HodledAsset> hodledAssets = new SortedSet<HodledAsset>();
                    List<WalletBalance> list = new List<WalletBalance>();
                    List<Transaction> transactions = SQLiteDatabaseManager.Database.Table<Transaction>().ToListAsync().Result;
                    if (transactions.Count != 0)
                    {
                        transactions.Sort();
                        Dictionary<string, FinancialStatementHelper> fshelper = new Dictionary<string, FinancialStatementHelper>();
                        List<string> wallets = new List<string>();
                        wallets.AddRange(transactions.Select(x => x.LocationStart));
                        wallets.AddRange(transactions.Select(x => x.LocationDestination));
                        foreach (string walletname in wallets.Distinct())
                        {
                            if (!string.IsNullOrWhiteSpace(walletname) && walletname != "Bank" && walletname != "Unbekanntes Wallet")
                            {
                                fshelper.Add(walletname, new FinancialStatementHelper());
                            }
                        }
                        if (!transactions.Any(x => x.TransactionValue == -1.0m))
                        {
                            try
                            {
                                foreach (Transaction transaction in transactions)
                                {
                                    transaction.Process(ref fshelper, ref hodledAssets);
                                }
                            }
                            catch{ InternalInstance.ContainsMissingValues = true; }
                        }
                        else { InternalInstance.ContainsMissingValues = true; }
                        if (!InternalInstance.ContainsMissingValues)
                        {
                            WalletBalance item;
                            bool isLeapYear;
                            foreach (HodledAsset asset in hodledAssets)
                            {
                                if (!asset.removed)
                                {
                                    if ((DateTime.IsLeapYear(asset.Received.Year) && asset.Received < new DateTime(asset.Received.Year, 2, 29)) || (DateTime.IsLeapYear(DateTime.UtcNow.Year) && DateTime.UtcNow > new DateTime(DateTime.UtcNow.Year, 3, 1)))
                                    {
                                        isLeapYear = true;
                                    }
                                    else
                                    {
                                        isLeapYear = false;
                                    }
                                    bool taxfree = asset.Received.IsWithinTimeSpan(DateTime.UtcNow, new TimeSpan(isLeapYear ? -366 : -365, 0, 0, 0));
                                    try
                                    {
                                        item = list.First(x => x.Asset == asset.Asset && x.IsTaxfree == taxfree);
                                        item.CurrentAmount += (double)asset.CurrentAmount;
                                    }
                                    catch
                                    {
                                        list.Add(new WalletBalance(asset.Location, asset.Asset, (double)asset.CurrentAmount, taxfree));
                                    }
                                }
                            }
                            list.ForEach(currentAssets.Add);
                        }
                    }
                }
                return currentAssets.ToList<IBalanceViewer>();
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
                    InternalInstance.AddLog(Resources.Strings.ShutdownP1 + retries + Resources.Strings.ShutdownP2, Resources.Strings.Shutdown, false);
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
                    InternalInstance.AddLog(Resources.Strings.CachedSymbols, Resources.Strings.Initialization);
                    return;
                }
            }
            catch
            {
                InternalInstance.AddLog(Resources.Strings.NoCachedSymbols, Resources.Strings.Initialization);
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
                InternalInstance.AddLog(Resources.Strings.BinanceSymbols, Resources.Strings.Symbols);
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
                InternalInstance.AddLog(Resources.Strings.BinanceSymbolsEnd, Resources.Strings.Symbols);
            }));
            taskList.Add(Task.Run(async () =>
            {
                InternalInstance.AddLog(Resources.Strings.BitstampSymbols, Resources.Strings.Symbols);
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
                InternalInstance.AddLog(Resources.Strings.BitstampSymbolsEnd, Resources.Strings.Symbols);
            }));
            Task.Run(async () =>
            {
                await Task.Delay(30000);
                if (successedBinance == false || successedBitstamp == false)
                {
                    cancelSource.Cancel();
                    InternalInstance.AddLog(Resources.Strings.SymbolsFailed, Resources.Strings.Symbols);
                    throw new InvalidOperationException("Loading symbols failed");
                }
            });
            Task.WhenAll(taskList).Wait();
            if (successedBinance && successedBitstamp)
            {
                int i = SQLiteDatabaseManager.Database.InsertOrReplaceAsync(cachedData).Result;
                InternalInstance.AddLog(Resources.Strings.CacheUpdated, Resources.Strings.Symbols);
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
                else { InternalInstance.AddLog(resultBitstamp.Error.Message, Resources.Strings.FetchPrices); }
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
                    InternalInstance.AddLog(resultBinance.Error.Message, Resources.Strings.FetchPrices);
                }
            }));
            await Task.WhenAll(taskList);
        }

        private void RefreshBalanceValues()
        {
            if (BinancePrices != null && BinancePrices.Count > 0 && BitstampPrices != null && BitstampPrices.Count > 0 && CurrentAssets != null && CurrentAssets.Count > 0)
            {
                double conversionRateBTCBinance = 1.0;
                double conversionRateBTCBitstamp = 1.0;
                switch (currency)
                {
                    case Currency.USDollar:
                        throw new NotImplementedException("Only supports EUR at the moment.");
                        //conversionRateBTCBinance = (double)BinancePrices.First(x => x.Symbol == "BTCUSDT").Price;
                        //conversionRateBTCBitstamp = (double)BitstampPrices.First(x => x.Pair == "BTC/USD").Last;
                        //break;
                    case Currency.Euro:
                        conversionRateBTCBinance = (double)BinancePrices.First(x => x.Symbol == "BTCEUR").Price;
                        conversionRateBTCBitstamp = (double)BitstampPrices.First(x => x.Pair == "BTC/EUR").Last;
                        break;
                }
                Parallel.ForEach(currentAssets, balance =>
                {
                    try
                    {
                        double conversionRate = 1.0;
                        switch (InternalInstance.ExchangeWallets.First(x => x.WalletName == balance.Wallet).Exchange)
                        {
                            case Exchange.Binance:
                                conversionRate = (double)BinancePrices.First(x => x.Symbol == balance.Asset + "BTC").Price;
                                balance.CurrentValue = balance.CurrentAmount * conversionRate * conversionRateBTCBinance;
                                break;
                            case Exchange.Bitstamp:
                                conversionRate = (double)BitstampPrices.First(x => x.Pair == balance.Asset.ToUpper() + "/BTC").Last;
                                balance.CurrentValue = balance.CurrentAmount * conversionRate * conversionRateBTCBitstamp;
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                    }
                    catch
                    {
                        //TODO
                    };
                });
                OnPropertyChanged("CurrentAssets");
            }
        }

        /// <inheritdoc/>
        public async void CreateWallet(string walletName, Exchange exchange, string csvFilePathTransactions, string csvFilePathWithdrawalDeposits, string csvFilePathDistribution)
        {
            InternalInstance.IsBusy = true;
            if (string.IsNullOrWhiteSpace(csvFilePathTransactions) || string.IsNullOrWhiteSpace(csvFilePathWithdrawalDeposits) || string.IsNullOrWhiteSpace(csvFilePathDistribution))
            {
                throw new ArgumentException("File paths can't be empty or whitespace.");
            }
            else
            {
                var resultW = await SQLiteDatabaseManager.Database.FindAsync<ExchangeWallet>(x => x.WalletName == walletName);
                if (resultW == null)
                {
                    IExchangeWalletViewer wallet = new ExchangeWallet(walletName, exchange, -1);
                    await SQLiteDatabaseManager.Database.InsertAsync(wallet);
                    ExchangeWallets.Add(wallet);
                    wallet.ImportFromCSV(csvFilePathTransactions, csvFilePathWithdrawalDeposits, csvFilePathDistribution);
                }
                else
                {
                    throw new InvalidOperationException("Wallet Name already exists.");
                }
            }
            InternalInstance.IsBusy = false;
        }
        /// <inheritdoc/>
        public async void CreateWallet(string walletName, Exchange exchange, string exchangeApiKey, string exchangeApiSecret)
        {
            InternalInstance.IsBusy = true;
            if (string.IsNullOrWhiteSpace(exchangeApiKey) || string.IsNullOrWhiteSpace(exchangeApiSecret))
            {
                throw new ArgumentException("API Credentials can't be empty or whitespace.");
            }
            else
            {
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
                                else throw new InvalidOperationException("Wrong credentials or unsufficent permissions");
                            }
                            catch
                            {
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
                                var result = await client.Api.Private.GetUserTransactionsAsync();
                                if (!result.Success)
                                {
                                    //Bitstamp misses a comparable method as binance has.
                                    throw new InvalidOperationException(result.Error.Message);
                                }
                            }
                            catch
                            {
                                throw new InvalidOperationException("Can't reach exchange. Please check connectivity.");
                            }
                        }
                        break;
                    default:
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
                    await taskQueue.Enqueue(async () => await wallet.SynchronizeWallet());
                }
                else
                {
                    throw new InvalidOperationException("Wallet Name or API Key already exists.");
                }
            }
            InternalInstance.IsBusy = false;
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
                Parallel.ForEach(transactions, async(transaction) =>
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
        /// <summary>
        /// Adds a new transaction to the Collection and Database and manipulate the AssetParts in the Wallets
        /// </summary>
        internal void AddNewTransaction(Transaction transaction)
        {
            taskQueue.Enqueue(async () => await newTransactionTask(transaction));
        }
        /// <summary>
        /// New Transaction Task - called by the public AddNewTransaction-Method
        /// </summary>
        private async Task newTransactionTask(Transaction transaction)
        {
            Transactions.Add(transaction);
            await SQLiteDatabaseManager.Database.InsertAsync(transaction);
#if DEBUG
            switch (transaction.TransactionType)
            {
                case Enums.TransactionType.Buy:
                case Enums.TransactionType.Sell:
                    InternalInstance.AddLog(Resources.Strings.NewTransaction + transaction.GetTradingPair(), Resources.Strings.TransactionTask);
                    break;
                default:
                    InternalInstance.AddLog(Resources.Strings.NewTransaction + transaction.TransactionType.ToString() + " " + transaction.AssetStart, Resources.Strings.TransactionTask);
                    break;
            }
#endif
        }
        /// <inheritdoc/>
        public void LoadMissingTransactionValues()
        {
            taskQueue.Enqueue(async () => await loadMissingTransactionValuesTask());
        }
        /// <summary>
        /// Task Execution of LoadMissingTransactionValues that can be enqueued
        /// </summary>
        private async Task loadMissingTransactionValuesTask()
        {
#if DEBUG
            Console.WriteLine("Load missing TransactionValue started.");
#endif
            InternalInstance.IsBusy = true;
            List<ExchangeWallet> exchWallets = await SQLiteDatabaseManager.Database.Table<ExchangeWallet>().ToListAsync();
            List<Transaction> transferTransactions = new List<Transaction>();
            foreach (Transaction t in Transactions.Where(x => string.IsNullOrWhiteSpace(x.LocationDestination) || string.IsNullOrWhiteSpace(x.LocationStart)))
            {
                transferTransactions.Add(t);
            }
            foreach (Transaction transaction in transferTransactions)
            {
                if (transaction.TransactionType == Enums.TransactionType.BankDeposit)
                {
                    transaction.LocationStart = "Bank";
                }
                else if (transaction.TransactionType == Enums.TransactionType.BankWithdrawal)
                {
                    transaction.LocationDestination = "Bank";
                }
                else
                {
                    foreach (Transaction compare in transferTransactions)
                    {
                        if (transaction.TransactionID != compare.TransactionID) //check if both are equal
                        {
                            if (string.IsNullOrWhiteSpace(transaction.LocationStart)) // = deposit; search for matching withdrawal
                            {
                                if (transaction.AmountDestination == compare.AmountStart && transaction.AssetDestination == compare.AssetStart && transaction.TransactionTime.IsWithinTimeSpan(compare.TransactionTime, new TimeSpan(1, 0, 0, 0)))
                                {
                                    transaction.LocationStart = compare.LocationStart;
                                    compare.LocationDestination = transaction.LocationDestination;
                                }
                            }
                            else if (string.IsNullOrWhiteSpace(transaction.LocationDestination)) // = withdrawal; search for matching deposit
                            {
                                if (transaction.AmountStart == compare.AmountDestination && transaction.AssetStart == compare.AssetDestination && transaction.TransactionTime.IsWithinTimeSpan(compare.TransactionTime, new TimeSpan(-1, 0, 0, 0)))
                                {
                                    transaction.LocationDestination = compare.LocationDestination;
                                    compare.LocationStart = transaction.LocationStart;
                                }
                            }
                        }
                    }
                }
            }
            await SQLiteDatabaseManager.Database.UpdateAllAsync(transferTransactions);
            List<Transaction> transactions = new List<Transaction>();
            foreach (Transaction t in Transactions.Where(x => x.TransactionValue == -1.0m || (x.FeeValue == 0.0m && x.FeeAmount != 0.0m)))
            {
                transactions.Add(t);
            }

            #region Create Requests
            //Aggregate DateTimes for Value API Calls with maximize efficency to reduce API calls by using duplicates
            SortedSet<ExchangeRateRequest> bitstampSet = new SortedSet<ExchangeRateRequest>();
            SortedSet<ExchangeRateRequest> binanceSet = new SortedSet<ExchangeRateRequest>();

            foreach (Transaction transaction in transactions)
            {
                Exchange exchange = exchWallets.First(x => x.WalletName == transaction.LocationStart || x.WalletName == transaction.LocationDestination).Exchange;
                switch (exchange)
                {
                    //TODO Add more exchanges here
                    case Exchange.Binance:
                        #region FeeAssets
                        if (!string.IsNullOrWhiteSpace(transaction.FeeAsset) && transaction.FeeValue == 0.0m)
                        {
                            if (transaction.FeeAsset == "EUR" || transaction.FeeAsset.IsEURstablecoin())
                            {
                                transaction.FeeValue = transaction.FeeAmount;
                            }
                            else if (transaction.FeeAsset == "BTC")  //If Asset is BTC
                            {
                                if (!bitstampSet.Any(x => x.Asset == "BTC" && x.BaseAsset == "EUR" && transaction.TransactionTime - x.DateTime <= new TimeSpan(16, 39, 59)))  //...and entry does not already exists
                                {
                                    bitstampSet.Add(new ExchangeRateRequest(transaction.TransactionTime.AddHours(-1), "BTC", "EUR"));
                                }
                            }
                            else if (transaction.FeeAsset == "USD" || transaction.FeeAsset.IsUSDStablecoin())  //If Asset is an USD equivalent
                            {
                                if (!bitstampSet.Any(x => x.Asset == "USD" && x.BaseAsset == "EUR" && transaction.TransactionTime - x.DateTime <= new TimeSpan(16, 39, 59)))  //...and entry does not already exists
                                {
                                    bitstampSet.Add(new ExchangeRateRequest(transaction.TransactionTime.AddHours(-1), "USD", "EUR"));
                                }
                            }
                            else  //Else request feeAsset/BTC-Pair from Binance and BTC/EUR-Pair from Bitstamp
                            {
                                if ((!binanceSet.Any(x => x.Asset == transaction.FeeAsset && transaction.TransactionTime - x.DateTime <= new TimeSpan(16, 39, 59))) && BinanceSymbols.Any(x => x.Name == (transaction.FeeAsset + "BTC")))  //...and entry does not already exists + is a valid symbol
                                {
                                    binanceSet.Add(new ExchangeRateRequest(transaction.TransactionTime.AddHours(-1), transaction.FeeAsset, "BTC"));
                                }
                                if (!bitstampSet.Any(x => x.Asset == "BTC" && x.BaseAsset == "EUR" && transaction.TransactionTime - x.DateTime <= new TimeSpan(16, 39, 59)))  //...and entry does not already exists
                                {
                                    bitstampSet.Add(new ExchangeRateRequest(transaction.TransactionTime.AddHours(-1), "BTC", "EUR"));
                                }
                            }
                        }
                        #endregion
                        #region TradeAssets
                        if (transaction.TransactionValue == -1)
                        {
                            if ((transaction.AssetStart == "EUR" || transaction.AssetStart.IsEURstablecoin()) && transaction.AmountStart != 0.0m)
                            {
                                transaction.TransactionValue = transaction.AmountStart;
                            }
                            else if ((transaction.AssetDestination == "EUR" || transaction.AssetDestination.IsEURstablecoin()) && transaction.AmountDestination != 0.0m)
                            {
                                transaction.TransactionValue = transaction.AmountDestination;
                            }
                            else if (transaction.AssetStart == "BTC" || transaction.AssetDestination == "BTC")  //If one of Assets is BTC
                            {
                                if (!bitstampSet.Any(x => x.Asset == "BTC" && x.BaseAsset == "EUR" && transaction.TransactionTime - x.DateTime <= new TimeSpan(16, 39, 59)))  //...and entry does not already exists
                                {
                                    bitstampSet.Add(new ExchangeRateRequest(transaction.TransactionTime.AddHours(-1), "BTC", "EUR"));
                                }
                            }
                            else if (transaction.AssetStart == "USD" || transaction.AssetDestination == "USD" || transaction.AssetStart.IsUSDStablecoin() || transaction.AssetDestination.IsUSDStablecoin())  //If one of Assets is an USD equivalent
                            {
                                if (!bitstampSet.Any(x => x.Asset == "USD" && x.BaseAsset == "EUR" && transaction.TransactionTime - x.DateTime <= new TimeSpan(16, 39, 59)))  //...and entry does not already exists
                                {
                                    bitstampSet.Add(new ExchangeRateRequest(transaction.TransactionTime.AddHours(-1), "USD", "EUR"));
                                }
                            }
                            else  //Else request startAsset/BTC-Pair from Binance and BTC/EUR-Pair from Bitstamp - except if not a valid pair exists, then use destinationAsset/BTC
                            {
                                if ((!binanceSet.Any(x => x.Asset == transaction.AssetStart && transaction.TransactionTime - x.DateTime <= new TimeSpan(16, 39, 59))) && BinanceSymbols.Any(x => x.Name == (transaction.AssetStart + "BTC")))  //...and entry does not already exists + is a valid symbol
                                {
                                    binanceSet.Add(new ExchangeRateRequest(transaction.TransactionTime.AddHours(-1), transaction.AssetStart, "BTC"));
                                }
                                else if ((!binanceSet.Any(x => x.Asset == transaction.AssetDestination && transaction.TransactionTime - x.DateTime <= new TimeSpan(16, 39, 59))) && BinanceSymbols.Any(x => x.Name == (transaction.AssetDestination + "BTC")))  //...and entry does not already exists + is a valid symbol
                                {
                                    binanceSet.Add(new ExchangeRateRequest(transaction.TransactionTime.AddHours(-1), transaction.AssetDestination, "BTC"));
                                }
                                if (!bitstampSet.Any(x => x.Asset == "BTC" && x.BaseAsset == "EUR" && transaction.TransactionTime - x.DateTime <= new TimeSpan(16, 39, 59)))  //...and entry does not already exists
                                {
                                    bitstampSet.Add(new ExchangeRateRequest(transaction.TransactionTime.AddHours(-1), "BTC", "EUR"));
                                }
                            }
                        }
                        #endregion
                        break;
                    case Exchange.Bitstamp:
                        #region FeeAssets
                        if (!string.IsNullOrWhiteSpace(transaction.FeeAsset) && transaction.FeeValue == 0.0m)
                        {
                            if (transaction.FeeAsset == "EUR" || transaction.FeeAsset.IsEURstablecoin())
                            {
                                transaction.FeeValue = transaction.FeeAmount;
                            }
                            else if (transaction.FeeAsset == "USD" || transaction.FeeAsset.IsUSDStablecoin())  //If Asset is an USD equivalent
                            {
                                if (!bitstampSet.Any(x => x.Asset == "USD" && x.BaseAsset == "EUR" && transaction.TransactionTime - x.DateTime <= new TimeSpan(16, 39, 59)))  //...and entry does not already exists
                                {
                                    bitstampSet.Add(new ExchangeRateRequest(transaction.TransactionTime.AddHours(-1), "USD", "EUR"));
                                }
                            }
                            else  //Else request feeAsset/EUR-Pair from Binance and BTC/EUR-Pair from Bitstamp
                            {
                                if (!bitstampSet.Any(x => x.Asset == transaction.FeeAsset && x.BaseAsset == "EUR" && transaction.TransactionTime - x.DateTime <= new TimeSpan(16, 39, 59)))  //...and entry does not already exists
                                {
                                    bitstampSet.Add(new ExchangeRateRequest(transaction.TransactionTime.AddHours(-1), transaction.FeeAsset, "EUR"));
                                }
                            }
                        }
                        #endregion
                        #region TradeAssets
                        if (transaction.TransactionValue == -1)
                        {
                            if ((transaction.AssetStart == "EUR" || transaction.AssetStart.IsEURstablecoin()) && transaction.AmountStart != 0.0m)
                            {
                                transaction.TransactionValue = transaction.AmountStart;
                            }
                            else if ((transaction.AssetDestination == "EUR" || transaction.AssetDestination.IsEURstablecoin()) && transaction.AmountDestination != 0.0m)
                            {
                                transaction.TransactionValue = transaction.AmountDestination;
                            }
                            else if (transaction.AssetStart == "BTC" || transaction.AssetDestination == "BTC")  //If one of Assets is BTC
                            {
                                if (!bitstampSet.Any(x => x.Asset == "BTC" && x.BaseAsset == "EUR" && transaction.TransactionTime - x.DateTime <= new TimeSpan(16, 39, 59)))  //...and entry does not already exists
                                {
                                    bitstampSet.Add(new ExchangeRateRequest(transaction.TransactionTime.AddHours(-1), "BTC", "EUR"));
                                }
                            }
                            else if (transaction.AssetStart == "USD" || transaction.AssetDestination == "USD" || transaction.AssetStart.IsUSDStablecoin() || transaction.AssetDestination.IsUSDStablecoin())  //If one of Assets is an USD equivalent
                            {
                                if (!bitstampSet.Any(x => x.Asset == "USD" && x.BaseAsset == "EUR" && transaction.TransactionTime - x.DateTime <= new TimeSpan(16, 39, 59)))  //...and entry does not already exists
                                {
                                    bitstampSet.Add(new ExchangeRateRequest(transaction.TransactionTime.AddHours(-1), "USD", "EUR"));
                                }
                            }
                            else  //Else request startAsset/EUR-Pair if not null or eur 
                            {
                                if (transaction.AssetStart != null && transaction.AssetStart != "EUR")
                                {
                                    if (!bitstampSet.Any(x => x.Asset == transaction.AssetStart && x.BaseAsset == "EUR" && transaction.TransactionTime - x.DateTime <= new TimeSpan(16, 39, 59)))  //...and entry does not already exists
                                    {
                                        bitstampSet.Add(new ExchangeRateRequest(transaction.TransactionTime.AddHours(-1), transaction.AssetStart, "EUR"));
                                    }
                                }
                                else if (transaction.AssetDestination != null && transaction.AssetDestination != "EUR")
                                {
                                    if (!bitstampSet.Any(x => x.Asset == transaction.AssetDestination && x.BaseAsset == "EUR" && transaction.TransactionTime - x.DateTime <= new TimeSpan(16, 39, 59)))  //...and entry does not already exists
                                    {
                                        bitstampSet.Add(new ExchangeRateRequest(transaction.TransactionTime.AddHours(-1), transaction.AssetDestination, "EUR"));
                                    }
                                }
                            }
                        }
                        #endregion
                        break;
                    default: break;
                }
            }
            #endregion

            #region API Calls
            List<Task> taskList = new List<Task>();
            #region Bitstamp
            taskList.Add(Task.Run(async () =>
                {
                    IBitstampClient client = new BitstampClient();
                    foreach (ExchangeRateRequest request in bitstampSet)
                    {
                        var result = await client.Api.Public.GetOHLCDataAsync(request.Asset == "BTC" ? "btceur" : request.Asset == "USD" ? "eurusd" : request.Asset.ToLower() + request.BaseAsset.ToLower(), 60, 1000, request.DateTime, request.DateTime.AddHours(16).AddMinutes(40));
                        if (result.Success)
                        {
                            List<ExchangeRate> temp = new List<ExchangeRate>();
                            foreach (IBitstampKline kline in result.Data.Data.OHLC)
                            {
                                temp.Add(new ExchangeRate(kline.Timestamp, kline.Timestamp.AddMilliseconds(59999), (kline.High + kline.Low + kline.Open + kline.Close) / 4));
                            }
                            request.Rates = temp;
                        }
                        else
                        {
                            InternalInstance.AddLog(Resources.Strings.Error + "@Bitstamp-GetOHLC: " + request.Asset + request.BaseAsset + Environment.NewLine + result.Error.Message);
                        }
                        await Task.Delay(100);
                    }
                }));
            #endregion
            #region Binance
            taskList.Add(Task.Run(async () =>
            {
                IBinanceRestClient Client = new BinanceRestClient();
                foreach (ExchangeRateRequest request in binanceSet)
                {
                    var result = await Client.SpotApi.ExchangeData.GetKlinesAsync(request.Asset + request.BaseAsset, KlineInterval.OneMinute, request.DateTime, request.DateTime.AddHours(16).AddMinutes(40), 1000); //16h40min == 1000 min -> 1000 limited klines @ 1 Minute Interval
                    if (result.Success)
                    {
                        List<ExchangeRate> temp = new List<ExchangeRate>();
                        foreach (IBinanceKline kline in result.Data)
                        {
                            temp.Add(new ExchangeRate(kline.OpenTime, kline.CloseTime, (kline.HighPrice + kline.LowPrice + kline.OpenPrice + kline.ClosePrice) / 4));
                        }
                        request.Rates = temp;
                    }
                    else
                    {
                        InternalInstance.AddLog(Resources.Strings.Error + "@Binance-GetKlines: " + request.Asset + request.BaseAsset + Environment.NewLine + result.Error.Message);
                    }
                    await Task.Delay(900);
                }
            }));
            #endregion
            await Task.WhenAll(taskList);
            #endregion

            #region Add Informations from Requests to transactions
            Parallel.ForEach(transactions, (transaction) =>
            {
                Exchange exchange = exchWallets.First(x => x.WalletName == transaction.LocationStart || x.WalletName == transaction.LocationDestination).Exchange;
                switch (exchange)
                {
                    case Exchange.Binance:
                        #region TradeAssets
                        if (transaction.TransactionValue == -1.0m) //this occurs, if one of the assets was in EUR and was directly set
                        {
                            try
                            {
                                //If one Asset is BTC
                                if (transaction.AssetStart == "BTC" || transaction.AssetDestination == "BTC")
                                {
                                    var hasInformation = bitstampSet.First(x => x.Asset == "BTC" && x.BaseAsset == "EUR" && transaction.TransactionTime.IsWithinTimeSpan(x.DateTime, new TimeSpan(16, 39, 59)));
                                    var rate = hasInformation.Rates.First(x => x.OpenTime <= transaction.TransactionTime && x.CloseTime >= transaction.TransactionTime);
                                    if (transaction.AssetStart == "BTC")
                                    {
                                        transaction.TransactionValue = transaction.AmountStart * rate.MedianRate;
                                    }
                                    else transaction.TransactionValue = transaction.AmountDestination * rate.MedianRate;
                                }
                                //If one Asset is USD equivalent
                                else if (transaction.AssetStart == "USD" || transaction.AssetStart.IsUSDStablecoin() || transaction.AssetDestination == "USD" || transaction.AssetDestination.IsUSDStablecoin())
                                {
                                    var hasInformation = bitstampSet.First(x => x.Asset == "USD" && x.BaseAsset == "EUR" && transaction.TransactionTime.IsWithinTimeSpan(x.DateTime, new TimeSpan(16, 39, 59)));
                                    var rate = hasInformation.Rates.First(x => x.OpenTime <= transaction.TransactionTime && x.CloseTime >= transaction.TransactionTime);
                                    if (transaction.AssetStart == "USD" || transaction.AssetStart.IsUSDStablecoin())
                                    {
                                        transaction.TransactionValue = transaction.AmountStart / rate.MedianRate;
                                    }
                                    else transaction.TransactionValue = transaction.AmountDestination / rate.MedianRate;
                                }
                                //else, the startAsset/BTC-Pair was used
                                else
                                {
                                    var hasInformationBitstamp = bitstampSet.First(x => x.Asset == "BTC" && x.BaseAsset == "EUR" && transaction.TransactionTime.IsWithinTimeSpan(x.DateTime, new TimeSpan(16, 39, 59)));
                                    var btcEurRate = hasInformationBitstamp.Rates.First(x => x.OpenTime <= transaction.TransactionTime && x.CloseTime >= transaction.TransactionTime);

                                    try

                                    {
                                        var hasInformationBinance = binanceSet.First(x => x.Asset == transaction.AssetStart && x.BaseAsset == "BTC" && transaction.TransactionTime.IsWithinTimeSpan(x.DateTime, new TimeSpan(16, 39, 59)));
                                        var rateBinance = hasInformationBinance.Rates.First(x => x.OpenTime <= transaction.TransactionTime && x.CloseTime >= transaction.TransactionTime);
                                        transaction.TransactionValue = transaction.AmountStart * rateBinance.MedianRate * btcEurRate.MedianRate;
                                    }
                                    catch (Exception) //rare occasions, where there is no valid startAsset/BTC pair on binance -> search for the destinationAsset/BTC pair
                                    {
                                        var hasInformationBinance = binanceSet.First(x => x.Asset == transaction.AssetDestination && x.BaseAsset == "BTC" && transaction.TransactionTime.IsWithinTimeSpan(x.DateTime, new TimeSpan(16, 39, 59)));
                                        var rateBinance = hasInformationBinance.Rates.First(x => x.OpenTime <= transaction.TransactionTime && x.CloseTime >= transaction.TransactionTime);
                                        transaction.TransactionValue = transaction.AmountDestination * rateBinance.MedianRate * btcEurRate.MedianRate;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                switch (transaction.TransactionType)
                                {
                                    case Enums.TransactionType.Buy:
                                    case Enums.TransactionType.Sell:
                                        InternalInstance.AddLog(Resources.Strings.Error + "@ParallelAddInformation: " + transaction.GetTradingPair() + " @ " + transaction.TransactionTime.ToString() + " ----- " + ex.Message);
                                        break;
                                    case Enums.TransactionType.Distribution:
                                        transaction.TransactionValue = 0.0m; //Some Assets were distributed on Binance, before there were trading pairs for them -> add TransactionValue = 0, since Distribution isn't counted as buy
                                        break;
                                    default:
                                        InternalInstance.AddLog(Resources.Strings.Error + "@ParallelAddInformation: " + transaction.TransactionType.ToString() + " " + transaction.AssetStart + " ----- " + ex.Message);
                                        break;
                                }
                            }
                        }
                        #endregion
                        #region FeeAssets
                        if (!string.IsNullOrWhiteSpace(transaction.FeeAsset) && transaction.FeeValue == 0.0m)
                        {
                            try
                            {
                                //If Asset is BTC
                                if (transaction.FeeAsset == "BTC")
                                {
                                    var hasInformation = bitstampSet.First(x => x.Asset == "BTC" && x.BaseAsset == "EUR" && transaction.TransactionTime.IsWithinTimeSpan(x.DateTime, new TimeSpan(16, 39, 59)));
                                    var rate = hasInformation.Rates.First(x => x.OpenTime <= transaction.TransactionTime && x.CloseTime >= transaction.TransactionTime);
                                    transaction.FeeValue = transaction.FeeAmount * rate.MedianRate;
                                }
                                //If Asset is USD equivalent
                                else if (transaction.FeeAsset == "USD" || transaction.FeeAsset.IsUSDStablecoin())
                                {
                                    var hasInformation = bitstampSet.First(x => x.Asset == "USD" && x.BaseAsset == "EUR" && transaction.TransactionTime.IsWithinTimeSpan(x.DateTime, new TimeSpan(16, 39, 59)));
                                    var rate = hasInformation.Rates.First(x => x.OpenTime <= transaction.TransactionTime && x.CloseTime >= transaction.TransactionTime);
                                    transaction.FeeValue = transaction.FeeAmount / rate.MedianRate;
                                }
                                //else, the feeAsset/BTC-Pair was used
                                else
                                {
                                    var hasInformationBitstamp = bitstampSet.First(x => x.Asset == "BTC" && x.BaseAsset == "EUR" && transaction.TransactionTime.IsWithinTimeSpan(x.DateTime, new TimeSpan(16, 39, 59)));
                                    var btcEurRate = hasInformationBitstamp.Rates.First(x => x.OpenTime <= transaction.TransactionTime && x.CloseTime >= transaction.TransactionTime);
                                    var hasInformationBinance = binanceSet.First(x => x.Asset == transaction.FeeAsset && x.BaseAsset == "BTC" && transaction.TransactionTime.IsWithinTimeSpan(x.DateTime, new TimeSpan(16, 39, 59)));
                                    var rateBinance = hasInformationBinance.Rates.First(x => x.OpenTime <= transaction.TransactionTime && x.CloseTime >= transaction.TransactionTime);
                                    transaction.FeeValue = transaction.FeeAmount * rateBinance.MedianRate * btcEurRate.MedianRate;
                                }
                            }
                            catch (Exception ex)
                            {
                                switch (transaction.TransactionType)
                                {
                                    case Enums.TransactionType.Buy:
                                    case Enums.TransactionType.Sell:
                                        InternalInstance.AddLog(Resources.Strings.Error + "@ParallelAddInformationFees: " + transaction.GetTradingPair() + " @ " + transaction.TransactionTime.ToString() + " ----- " + ex.Message);
                                        break;
                                    default:
                                        InternalInstance.AddLog(Resources.Strings.Error + "@ParallelAddInformationFees: " + transaction.TransactionType.ToString() + " " + transaction.AssetStart + " ----- " + ex.Message);
                                        break;
                                }
                            }
                        }
                        #endregion
                        break;
                    case Exchange.Bitstamp:
                        #region TradeAssets
                        if (transaction.TransactionValue == -1.0m) //this occurs, if one of the assets was in EUR and was directly set
                        {
                            try
                            {
                                //If one Asset is BTC
                                if (transaction.AssetStart == "BTC" || transaction.AssetDestination == "BTC")
                                {
                                    var hasInformation = bitstampSet.First(x => x.Asset == "BTC" && x.BaseAsset == "EUR" && transaction.TransactionTime.IsWithinTimeSpan(x.DateTime, new TimeSpan(16, 39, 59)));
                                    var rate = hasInformation.Rates.First(x => x.OpenTime <= transaction.TransactionTime && x.CloseTime >= transaction.TransactionTime);
                                    if (transaction.AssetStart == "BTC")
                                    {
                                        transaction.TransactionValue = transaction.AmountStart * rate.MedianRate;
                                    }
                                    else transaction.TransactionValue = transaction.AmountDestination * rate.MedianRate;
                                }
                                //If one Asset is USD equivalent
                                else if (transaction.AssetStart == "USD" || transaction.AssetStart.IsUSDStablecoin() || transaction.AssetDestination == "USD" || transaction.AssetDestination.IsUSDStablecoin())
                                {
                                    var hasInformation = bitstampSet.First(x => x.Asset == "USD" && x.BaseAsset == "EUR" && transaction.TransactionTime.IsWithinTimeSpan(x.DateTime, new TimeSpan(16, 39, 59)));
                                    var rate = hasInformation.Rates.First(x => x.OpenTime <= transaction.TransactionTime && x.CloseTime >= transaction.TransactionTime);
                                    if (transaction.AssetStart == "USD" || transaction.AssetStart.IsUSDStablecoin())
                                    {
                                        transaction.TransactionValue = transaction.AmountStart / rate.MedianRate;
                                    }
                                    else transaction.TransactionValue = transaction.AmountDestination / rate.MedianRate;
                                }
                                //else, the startAsset/EUR-Pair or the destinationAsset/EUR-Pair was used
                                else if (transaction.AssetStart != null && transaction.AssetStart != "EUR")
                                {
                                    var hasInformationBitstamp = bitstampSet.First(x => x.Asset == transaction.AssetStart && x.BaseAsset == "EUR" && transaction.TransactionTime.IsWithinTimeSpan(x.DateTime, new TimeSpan(16, 39, 59)));
                                    var bitstampRate = hasInformationBitstamp.Rates.First(x => x.OpenTime <= transaction.TransactionTime && x.CloseTime >= transaction.TransactionTime);
                                    transaction.TransactionValue = transaction.AmountStart * bitstampRate.MedianRate;
                                }
                                else
                                {
                                    var hasInformationBitstamp = bitstampSet.First(x => x.Asset == transaction.AssetDestination && x.BaseAsset == "EUR" && transaction.TransactionTime.IsWithinTimeSpan(x.DateTime, new TimeSpan(16, 39, 59)));
                                    var bitstampRate = hasInformationBitstamp.Rates.First(x => x.OpenTime <= transaction.TransactionTime && x.CloseTime >= transaction.TransactionTime);
                                    transaction.TransactionValue = transaction.AmountDestination * bitstampRate.MedianRate;
                                }
                            }
                            catch (Exception ex)
                            {
                                switch (transaction.TransactionType)
                                {
                                    case Enums.TransactionType.Buy:
                                    case Enums.TransactionType.Sell:
                                        InternalInstance.AddLog(Resources.Strings.Error + "@ParallelAddInformation: " + transaction.GetTradingPair() + " @ " + transaction.TransactionTime.ToString() + " ----- " + ex.Message);
                                        break;
                                    case Enums.TransactionType.Distribution:
                                        transaction.TransactionValue = 0.0m;
                                        break;
                                    default:
                                        InternalInstance.AddLog(Resources.Strings.Error + "@ParallelAddInformation: " + transaction.TransactionType.ToString() + " " + transaction.AssetStart + " ----- " + ex.Message);
                                        break;
                                }
                            }
                        }
                        #endregion
                        #region FeeAssets
                        if (!string.IsNullOrWhiteSpace(transaction.FeeAsset) && transaction.FeeValue == 0.0m)
                        {
                            try
                            {
                                //If Asset is USD equivalent
                                if (transaction.FeeAsset == "USD" || transaction.FeeAsset.IsUSDStablecoin())
                                {
                                    var hasInformation = bitstampSet.First(x => x.Asset == "USD" && x.BaseAsset == "EUR" && transaction.TransactionTime.IsWithinTimeSpan(x.DateTime, new TimeSpan(16, 39, 59)));
                                    var rate = hasInformation.Rates.First(x => x.OpenTime <= transaction.TransactionTime && x.CloseTime >= transaction.TransactionTime);
                                    transaction.FeeValue = transaction.FeeAmount / rate.MedianRate;
                                }
                                //else, the feeAsset/BTC-Pair was used
                                else
                                {
                                    var hasInformationBitstamp = bitstampSet.First(x => x.Asset == transaction.FeeAsset && x.BaseAsset == "EUR" && transaction.TransactionTime.IsWithinTimeSpan(x.DateTime, new TimeSpan(16, 39, 59)));
                                    var rateBitstamp = hasInformationBitstamp.Rates.First(x => x.OpenTime <= transaction.TransactionTime && x.CloseTime >= transaction.TransactionTime);
                                    transaction.FeeValue = transaction.FeeAmount * rateBitstamp.MedianRate;
                                }
                            }
                            catch (Exception ex)
                            {
                                switch (transaction.TransactionType)
                                {
                                    case Enums.TransactionType.Buy:
                                    case Enums.TransactionType.Sell:
                                        InternalInstance.AddLog(Resources.Strings.Error + "@ParallelAddInformationFees: " + transaction.GetTradingPair() + " @ " + transaction.TransactionTime.ToString() + " ----- " + ex.Message);
                                        break;
                                    default:
                                        InternalInstance.AddLog(Resources.Strings.Error + "@ParallelAddInformationFees: " + transaction.TransactionType.ToString() + " " + transaction.AssetStart + " ----- " + ex.Message);
                                        break;
                                }
                            }
                        }
                        #endregion
                        break;
                }
            });
            #endregion

            await SQLiteDatabaseManager.Database.UpdateAllAsync(transactions);
            OnPropertyChanged("Transactions");
            InternalInstance.IsBusy = false;
#if DEBUG
            Console.WriteLine("Load missing TransactionValue ended.");
#endif
        }
    }
}