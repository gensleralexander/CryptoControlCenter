using Bitstamp.Net.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Bitstamp.Net.Objects
{
    /// <summary>
    /// Root-Class for Crypto Deposits and Withdrawals
    /// </summary>
    public class BitstampCryptoTransactions
    {
        [JsonProperty("ripple_iou_transactions")]
        public IEnumerable<BitstampCryptoTransaction> RippleIouTransactions { get; set; }

        [JsonProperty("deposits")]
        public IEnumerable<BitstampCryptoTransaction> Deposits { get; set; }

        [JsonProperty("withdrawals")]
        public IEnumerable<BitstampCryptoTransaction> Withdrawals { get; set; }

        public BitstampCryptoTransactions() { }
    }


    /// <summary>
    /// Class for Crypto Deposits and Withdrawals
    /// </summary>
    public class BitstampCryptoTransaction
    {
        /// <summary>
        /// Currency
        /// </summary>
        [JsonProperty("currency")]
        public string Currency { get; set; }
        /// <summary>
        /// Destination
        /// </summary>
        [JsonProperty("destinationAddress")]
        public string DestinationAddress { get; set; }
        /// <summary>
        /// Transaction Hash
        /// </summary>
        [JsonProperty("txid")]
        public string TransactionHash { get; set; }
        /// <summary>
        /// Amount of Currency
        /// </summary>
        [JsonProperty("amount")]
        public decimal Amount { get; set; }
        /// <summary>
        /// Transaction Time
        /// </summary>
        [JsonProperty("datetime"), JsonConverter(typeof(BitstampTimestampConverter))]
        public DateTime Datetime { get; set; }

        /// <summary>
        /// Konstruktor
        /// </summary>
        public BitstampCryptoTransaction() { }
    }
}