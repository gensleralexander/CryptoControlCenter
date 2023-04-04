using Bitstamp.Net.Enums;
using Newtonsoft.Json;

namespace Bitstamp.Net.Objects
{
    /// <summary>
    /// Exchange info
    /// </summary>
    public class BitstampTradingPairInfo
    {
        /// <summary>
        /// Name of the trading pair
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
        /// <summary>
        /// URL Symbol of Trading
        /// </summary>
        [JsonProperty("url_symbol")]
        public string URLSymbol { get; set; }
        /// <summary>
        /// Decimal precision for base currency (BTC/USD - base: BTC).
        /// </summary>
        [JsonProperty("base_decimals")]
        public int BaseDecimals { get; set; }
        /// <summary>
        /// Decimal precision for counter currency (BTC/USD - counter: USD).
        /// </summary>
        [JsonProperty("counter_decimals")]
        public int CounterDecimals { get; set; }
        /// <summary>
        /// Minimum order size.
        /// </summary>
        [JsonProperty("minimum_order")]
        public string MinimumOrder { get; set; }
        /// <summary>
        /// Trading engine status (Enabled/Disabled).
        /// </summary>
        [JsonProperty("trading")]
        public TradingStatus Status { get; set; }
        /// <summary>
        /// Trading pair description.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public BitstampTradingPairInfo() { }
    }
}
