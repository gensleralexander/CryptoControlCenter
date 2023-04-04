using Newtonsoft.Json;
using System.Collections.Generic;

namespace Bitstamp.Net.Objects
{
    public class OHLCData
    {
        /// <summary>
        /// The trading pair
        /// </summary>
        [JsonProperty("pair")]
        public string Pair { get; set; }
        /// <summary>
        /// Klines
        /// </summary>
        [JsonProperty("ohlc")]
        public IEnumerable<BitstampKline> OHLC { get; set; }
    }
}
