using CryptoControlCenter.Common.Enums;
using CryptoControlCenter.Common.Models.Interfaces;
using Newtonsoft.Json;
using SQLite;
using System;
using System.Text;

namespace CryptoControlCenter.Common.Models
{
    /// <summary>
    /// Class for Transaction Data
    /// </summary>
    [Table("Transaction")]
    public class Transaction : ITransactionViewer, IComparable
    {
        /// <inheritdoc />
        [PrimaryKey, AutoIncrement]
        public int TransactionID { get; set; }
        /// <inheritdoc />
        public string Wallet { get; set; }
        /// <inheritdoc />
        public DateTime TransactionTime { get; set; }
        /// <inheritdoc />
        public TransactionType TransactionType { get; set; }
        /// <inheritdoc />
        public string AssetStart { get; set; }
        /// <inheritdoc />
        public string AssetDestination { get; set; }
        /// <inheritdoc />
        public decimal AmountStart { get; set; }
        /// <inheritdoc />
        public decimal AmountDestination { get; set; }
        /// <inheritdoc />
        public string LocationStart { get; set; }
        /// <inheritdoc />
        public string LocationDestination { get; set; }
        /// <inheritdoc />
        public decimal TransferValue { get; set; }
        /// <inheritdoc />
        public decimal FeeAmount { get; set; }
        /// <inheritdoc />
        public string FeeAsset { get; set; }
        /// <inheritdoc />
        public decimal FeeValue { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Transaction() { }
        /// <summary>
        /// Constructor
        /// </summary>
        public Transaction(string walletName, DateTime transactionTime, TransactionType transactionType, string assetStart, string assetDestination, decimal amountStart, decimal amountDestination, string locationStart, string locationDestination, decimal transferValue, decimal feeAmount, string feeAsset)
        {
            Wallet = walletName;
            TransactionTime = transactionTime;
            TransactionType = transactionType;
            AssetStart = assetStart;
            AssetDestination = assetDestination;
            AmountStart = amountStart;
            AmountDestination = amountDestination;
            LocationStart = locationStart;
            LocationDestination = locationDestination;
            TransferValue = transferValue;
            FeeAmount = feeAmount;
            FeeAsset = feeAsset;
        }

        /// <inheritdoc />
        public string GetTradingPair()
        {
            switch (TransactionType)
            {
                case TransactionType.Buy:
                    return AssetDestination + "/" + AssetStart;
                case TransactionType.Sell:
                    return AssetStart + "/" + AssetDestination;
                default:
                    return string.Empty;
            }
        }

        /// <inheritdoc />
        public decimal? GetExchangeRate()
        {
            switch (TransactionType)
            {
                case TransactionType.Buy:
                    return AmountStart / AmountDestination;
                case TransactionType.Sell:
                case TransactionType.Dust:
                    return AmountDestination / AmountStart;
                default:
                    return null;
            }
        }
        /// <inheritdoc />
        public string GetExchangeRateString()
        {
            StringBuilder sb = new StringBuilder();
            switch (TransactionType)
            {
                case TransactionType.Buy:
                    sb.Append("1 ");
                    sb.Append(AssetDestination);
                    sb.Append(" = ");
                    sb.Append(decimal.Round((decimal)GetExchangeRate(), 8, MidpointRounding.ToEven));
                    sb.Append(" ");
                    sb.Append(AssetStart);
                    break;
                case TransactionType.Sell:
                    sb.Append("1 ");
                    sb.Append(AssetStart);
                    sb.Append(" = ");
                    sb.Append(decimal.Round((decimal)GetExchangeRate(), 8, MidpointRounding.ToEven));
                    sb.Append(" ");
                    sb.Append(AssetDestination);
                    break;
            }
            return sb.ToString();
        }
        /// <inheritdoc />
        public int CompareTo(object item)
        {
            if (TransactionTime != ((Transaction)item).TransactionTime)
            {
                return TransactionTime.CompareTo(((Transaction)item).TransactionTime);  //if TransactionTimes are not equal, compare and sort the transactions by this
            }
            else if (JsonConvert.SerializeObject(this) == JsonConvert.SerializeObject(item))
            {
                return 0;   //if Transaction times are equal, compare if both transactions have same values (is a duplicate)
            }
            else return 1;  //else return 1 -> item is "greater" means it gets sorted after this.
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(TransactionTime);
            sb.Append(" --- ");
            sb.Append(TransactionType);
            sb.Append("@");
            sb.Append(Wallet);
            sb.Append(" --- ");
            sb.Append(AssetStart);
            sb.Append(" to ");
            sb.Append(AssetDestination);
            return sb.ToString();
        }
    }
}