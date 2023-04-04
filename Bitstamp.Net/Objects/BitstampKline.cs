using System;
using Bitstamp.Net.Converters;
using Bitstamp.Net.Interfaces;
using Newtonsoft.Json;

namespace Bitstamp.Net.Objects
{
    /// <summary>
    /// Candlestick information for symbol
    /// </summary>
    public class BitstampKline : IBitstampKline
    {
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
        /// The price at which this candlestick closed
        /// </summary>
        [JsonProperty("close")]
        public decimal Close { get; set; }
        /// <summary>
        /// The price at which this candlestick opened
        /// </summary>
        [JsonProperty("open")]
        public decimal Open { get; set; }

        public BitstampKline() { }
    }
}