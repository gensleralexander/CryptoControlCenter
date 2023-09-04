namespace Bitstamp.Net
{
    /// <summary>
    /// Api Addresses
    /// </summary>
    public class BitstampApiAddresses
    {
        /// <summary>
        /// The address used by the BitstampClient for the Spot API
        /// </summary>
        public string RestClientAddress { get; set; } = "";
        /// <summary>
        /// The address used by the BitstampSocketClient for the Spot API
        /// </summary>
        public string SocketClientAddress { get; set; } = "";


        /// <summary>
        /// The default addresses to connect to the bitstamp.net API
        /// </summary>
        public static BitstampApiAddresses Default = new BitstampApiAddresses
        {
            RestClientAddress = "https://www.bitstamp.net",
            SocketClientAddress = "wss://ws.bitstamp.net",
        };
    }
}