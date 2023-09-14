using Bitstamp.Net.Converters;
using Newtonsoft.Json;
using System;

namespace Bitstamp.Net.Objects
{
    public class BitstampTicker
    {
        /// <summary>
        /// Name of the trading pair
        /// </summary>
        [JsonProperty("pair")]
        public string Pair { get; set; }
        /// <summary>
        /// The highest price in this candlestick
        /// </summary>
        [JsonProperty("high")]
        public decimal High { get; set; }
        /// <summary>
        /// The time this candlestick opened
        /// </summary>
        [JsonProperty("timestamp"), JsonConverter(typeof(BitstampTimestampConverter))]
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// The volume traded during this candlestick
        /// </summary>
        [JsonProperty("volume")]
        public decimal Volume { get; set; }
        /// <summary>
        /// The lowest price in this candlestick
        /// </summary>
        [JsonProperty("low")]
        public decimal Low { get; set; }
        /// <summary>
        /// The price last trade was done
        /// </summary>
        [JsonProperty("last")]
        public decimal Last { get; set; }
        /// <summary>
        /// The price at which this candlestick opened
        /// </summary>
        [JsonProperty("open")]
        public decimal Open { get; set; }
        /// <summary>
        /// Volume weighted average price
        /// </summary>
        [JsonProperty("vwap")]
        public decimal VWAP { get; set; }
        /// <summary>
        /// The current bid price
        /// </summary>
        [JsonProperty("bid")]
        public decimal Bid { get; set; }
        /// <summary>
        /// The current ask price
        /// </summary>
        [JsonProperty("ask")]
        public decimal Ask { get; set; }
        /// <summary>
        /// Side
        /// </summary>
        [JsonProperty("side")]
        public int Side { get; set; }
        /// <summary>
        /// Open 24h
        /// </summary>
        [JsonProperty("open_24")]
        public decimal Open24 { get; set; }
        /// <summary>
        /// Percent Change 24h
        /// </summary>
        [JsonProperty("percent_change_24")]
        public decimal? PercentChange24 { get; set; }

        public BitstampTicker()
        {
                
        }
    }
}
