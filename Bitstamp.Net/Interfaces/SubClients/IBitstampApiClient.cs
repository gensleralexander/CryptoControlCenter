using CryptoExchange.Net.Interfaces;
using System;

namespace Bitstamp.Net.Interfaces.SubClients
{
    public interface IBitstampApiClient : IDisposable, IRestApiClient
    {
        /// <summary>
        /// The factory for creating requests. Used for unit testing
        /// </summary>
        IRequestFactory RequestFactory { get; set; }

        /// <summary>
        /// Endpoints related to account settings, info or actions
        /// </summary>
        public IBitstampApiPrivateClient Private { get; }

        /// <summary>
        /// Endpoints related to retrieving market and system data
        /// </summary>
        public IBitstampApiPublicClient Public { get; }
    }
}
