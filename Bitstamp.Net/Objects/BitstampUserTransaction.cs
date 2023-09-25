using Bitstamp.Net.Enums;
using Newtonsoft.Json;
using System;

namespace Bitstamp.Net.Objects
{
    /// <summary>
    /// Class for BitstampUserTransactions
    /// 
    /// As class has a high need for maintenance (Bitstamp adding new coins = new properties), this class gets dynamic generated in Client-Method
    /// </summary>
    public class BitstampUserTransaction
    {
        internal string tradingPair;


        /// <summary>
        /// Transaction Time
        /// </summary>
        [JsonProperty("datetime")]
        public DateTime TransactionTime { get; set; }
        /// <summary>
        /// Fee Amount
        /// </summary>
        [JsonProperty("fee")]
        public decimal FeeAmount { get; set; }
        /// <summary>
        /// Fee Asset
        /// </summary>
        public string FeeAsset
        {
            get
            {
                if (Type == UserTransactionType.MarketTrade)
                {
                    var split = tradingPair.Split('_');
                    return split[1].ToUpper();
                }
                else if (Type == UserTransactionType.Withdrawal)
                {
                    return FromAsset;
                }
                else return string.Empty;
            }
        }
        /// <summary>
        /// Transaction ID
        /// </summary>
        [JsonProperty("id")]
        public long Id { get; set; }
        /// <summary>
        /// Transaction Type
        /// </summary>
        [JsonProperty("type")]
        public UserTransactionType Type { get; set; }
        /// <summary>
        /// Order ID
        /// </summary>
        [JsonProperty("order_id")]
        public long OrderId { get; set; }
        /// <summary>
        /// Exchange Rate
        /// </summary>
        public decimal ExchangeRate { get; set; }
        /// <summary>
        /// Start Asset
        /// </summary>
        public string FromAsset { get; set; }
        /// <summary>
        /// Destination Asset (if double-sided transaction)
        /// </summary>
        public string ToAsset { get; set; }
        /// <summary>
        /// Start Amount
        /// </summary>
        public decimal FromAmount { get; set; }
        /// <summary>
        /// To Amount (if double-sided transaction)
        /// </summary>
        public decimal ToAmount { get; set; }
        /// <summary>
        /// Indicates, if a market trade was Buy or Sell
        /// </summary>
        public bool IsBuyer
        {
            get
            {
                if (Type == UserTransactionType.MarketTrade)
                {
                    var pair = tradingPair.Split('_');
                    if (pair[0].ToUpper() == FromAsset)
                    {
                        return false;
                    }
                    else return true;
                }
                else return false;
            }
        }

        /// <summary>
        /// Construktor
        /// </summary>
        public BitstampUserTransaction() { }

    }
}