using CryptoControlCenter.Common.DataProvider;
using CryptoControlCenter.Common.Enums;
using CryptoControlCenter.Common.Models.Interfaces;
using SQLite;
using System;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CryptoControlCenter.Common.Models
{
    /// <summary>
    /// Base class for ExchangeWallet implementations
    /// </summary>
    [Table("ExchangeWallet")]
    public class ExchangeWallet : IExchangeWalletViewer
    {
        #region Properties
        /// <inheritdoc />
        [PrimaryKey]
        public string WalletName { get; set; }
        /// <inheritdoc />
        public string GetDescription()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Exchange.ToString());
            sb.Append(" (ID ");
            sb.Append(WalletName);
            sb.Append(")");
            return sb.ToString();
        }
        /// <inheritdoc />
        [Unique]
        public int SecureCredentialID { get; set; }
        /// <inheritdoc />
        public Exchange Exchange { get; set; }
        /// <inheritdoc />
        public DateTime? LastSync { get; set; }
        /// <summary>
        /// Current DataProvider
        /// </summary>
        private IDataProvider DataProvider { get; set; }
        #endregion
        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        public ExchangeWallet() { }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="walletName">PK Wallet Name</param>
        /// <param name="exchange">Wallet Exchange</param>
        /// <param name="secureCredentialID">FK of Secured Credential</param>
        public ExchangeWallet(string walletName, Exchange exchange, int secureCredentialID)
        {
            WalletName = walletName;
            Exchange = exchange;
            SecureCredentialID = secureCredentialID;

        }
        #endregion

        /// <inheritdoc />
        public async Task SynchronizeWallet(DateTime? startingPoint = null)
        {
            switch (Exchange)
            {
                case Exchange.Binance:
                    DataProvider = new BinanceDataProvider(this);
                    //await DataProvider.SynchronizeWallet(startingPoint); //Binance currently without Synchronization
                    break;
                case Exchange.Bitstamp:
                    DataProvider = new BitstampDataProvider(this);
                    await DataProvider.SynchronizeWallet(startingPoint);
                    break;
                default:
                    break;
            }
        }
        /// <inheritdoc />
        public void ImportFromCSV(string csvFilePathTransactions, string csvFilePathWithdrawalDeposits, string csvFilePathDistribution, DateTime? startingPoint)
        {
            switch (Exchange)
            {
                case Exchange.Binance:
                    DataProvider = new BinanceDataProvider(this);
                    DataProvider.ImportFromCSV(csvFilePathTransactions, csvFilePathWithdrawalDeposits, csvFilePathDistribution, startingPoint);
                    break;
                case Exchange.Bitstamp:
                    DataProvider = new BitstampDataProvider(this);
                    break;
                default:
                    break;
            }
        }
    }
}