using Bitstamp.Net.Interfaces.SubClients;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Interfaces;

namespace Bitstamp.Net.Interfaces
{
    /// <summary>
    /// Binance interface
    /// </summary>
    public interface IBitstampClient : IRestClient
    {
        /// <summary>
        /// Public endpoints
        /// </summary>
        IBitstampApiClient Api { get; }

        /// <summary>
        /// Set the API credentials for this client. All Api clients in this client will use the new credentials, regardless of earlier set options.
        /// </summary>
        /// <param name="credentials">The credentials to set</param>
        void SetApiCredentials(ApiCredentials credentials);
    }
}