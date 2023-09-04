using CryptoExchange.Net.Objects;

namespace Bitstamp.Net
{
    public class BitstampEnvironment : TradeEnvironment
    {
        /// <summary>
        /// Spot Rest API address
        /// </summary>
        public string SpotRestAddress { get; }

        /// <summary>
        /// Spot Socket Streams address
        /// </summary>
        public string SpotSocketStreamAddress { get; }

        internal BitstampEnvironment(string name, string spotRestAddress, string spotSocketStreamAddress) : base(name)
        {
            SpotRestAddress = spotRestAddress;
            SpotSocketStreamAddress = spotSocketStreamAddress;
        }

        public static BitstampEnvironment Live { get; } = new BitstampEnvironment(
            TradeEnvironmentNames.Live,
            BitstampApiAddresses.Default.RestClientAddress, 
            BitstampApiAddresses.Default.SocketClientAddress);
    }
}
