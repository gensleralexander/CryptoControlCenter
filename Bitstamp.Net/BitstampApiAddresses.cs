namespace Bitstamp.Net
{
    /// <summary>
    /// Api Addresses
    /// </summary>
    public class BitstampApiAddresses
    {
        /// <summary>
        /// The address used by the BinanceClient for the Spot API
        /// </summary>
        public string RestClientAddress { get; set; } = "";
        /// <summary>
        /// The address used by the BinanceSocketClient for the Spot API
        /// </summary>
        public string SocketClientAddress { get; set; } = "";


        /// <summary>
        /// The default addresses to connect to the binance.com API
        /// </summary>
        public static BitstampApiAddresses Default = new BitstampApiAddresses
        {
            RestClientAddress = "https://www.bitstamp.net",
            SocketClientAddress = "wss://ws.bitstamp.net",
        };
    }
}