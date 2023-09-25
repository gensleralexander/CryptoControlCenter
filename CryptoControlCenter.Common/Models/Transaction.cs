using CryptoControlCenter.Common.Database;
using CryptoControlCenter.Common.Enums;
using CryptoControlCenter.Common.Helper;
using CryptoControlCenter.Common.Models.Interfaces;
using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CryptoControlCenter.Common.Models
{
    /// <summary>
    /// Class for Transaction Data
    /// </summary>
    [Table("Transaction")]
    public class Transaction : AbstractPropertyChanged, ITransactionViewer, IComparable, IEditableObject
    {
        /// <inheritdoc />
        [PrimaryKey, AutoIncrement]
        public int TransactionID { get; set; }
        /// <inheritdoc />
        public string Wallet { get; set; }
        /// <inheritdoc />
        public DateTime TransactionTime { get; set; }
        private TransactionType transactionType;
        /// <inheritdoc />
        public TransactionType TransactionType
        {
            get { return transactionType; }
            set
            {
                transactionType = value;
                OnPropertyChanged();
            }
        }
        /// <inheritdoc />
        public string AssetStart { get; set; }
        /// <inheritdoc />
        public string AssetDestination { get; set; }
        /// <inheritdoc />
        public decimal AmountStart { get; set; }
        /// <inheritdoc />
        public decimal AmountDestination { get; set; }
        private string locationStart;
        /// <inheritdoc />
        public string LocationStart
        {
            get { return locationStart; }
            set
            {
                locationStart = value;
                OnPropertyChanged();
            }
        }
        private string locationDestination;
        /// <inheritdoc />
        public string LocationDestination
        {
            get { return locationDestination; }
            set
            {
                locationDestination = value;
                OnPropertyChanged();
            }
        }
        private decimal transactionValue;
        /// <inheritdoc />
        public decimal TransactionValue
        {
            get { return transactionValue; }
            set
            {
                transactionValue = value;
                OnPropertyChanged();
            }
        }
        private decimal feeAmount;
        /// <inheritdoc />
        public decimal FeeAmount
        {
            get { return feeAmount; }
            set
            {
                feeAmount = value;
                OnPropertyChanged();
            }
        }
        private string feeAsset;
        /// <inheritdoc />
        public string FeeAsset
        {
            get { return feeAsset; }
            set
            {
                feeAsset = value;
                OnPropertyChanged();
            }
        }
        private decimal feeValue;
        /// <inheritdoc />
        public decimal FeeValue
        {
            get { return feeValue; }
            set
            {
                feeValue = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public Transaction() { }
        /// <summary>
        /// Constructor
        /// </summary>
        public Transaction(string walletName, DateTime transactionTime, TransactionType transactionType, string assetStart, string assetDestination, decimal amountStart, decimal amountDestination, string locationStart, string locationDestination, decimal transactionValue, decimal feeAmount, string feeAsset)
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
            TransactionValue = transactionValue;
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



        //IEditable
        private Dictionary<string, object> storedValues;
        protected Dictionary<string, object> BackUp()
        {
            var dict = new Dictionary<string, object>();
            var itemProperties = this.GetType().GetTypeInfo().DeclaredProperties;

            foreach (var pDescriptor in itemProperties)
            {
                if (pDescriptor.CanWrite)
                    dict.Add(pDescriptor.Name, pDescriptor.GetValue(this));
            }
            return dict;
        }

        public void BeginEdit()
        {
            this.storedValues = this.BackUp();
        }

        public void CancelEdit()
        {
            if (this.storedValues == null)
                return;

            foreach (var item in this.storedValues)
            {
                var itemProperties = this.GetType().GetTypeInfo().DeclaredProperties;
                var pDesc = itemProperties.FirstOrDefault(p => p.Name == item.Key);

                if (pDesc != null)
                    pDesc.SetValue(this, item.Value);
            }
        }

        public async void EndEdit()
        {
            if (this.storedValues != null)
            {
                await SQLiteDatabaseManager.Database.UpdateAsync(this);
                this.storedValues.Clear();
                this.storedValues = null;
            }
        }
    }
}