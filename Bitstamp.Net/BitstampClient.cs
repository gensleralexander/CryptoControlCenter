using Bitstamp.Net.Interfaces;
using Bitstamp.Net.Interfaces.SubClients;
using Bitstamp.Net.Objects;
using CryptoExchange.Net;
using CryptoExchange.Net.Authentication;
using Microsoft.Extensions.Logging;
using System;

namespace Bitstamp.Net
{
    /// <summary>
    /// Client providing access to the Bitstam REST Api
    /// </summary>
    public class BitstampClient : BaseRestClient, IBitstampClient
    {
        /// <summary>
        /// Public endpoints
        /// </summary>
        public IBitstampApiClient Api { get; }


        #region constructor/destructor
        /// <summary>
        /// Create a new instance of BitstampClient using the default options
        /// </summary>
        public BitstampClient() : this(null, null)
        {
        }
        /// <summary>
        /// Create a new instance of BitstampClient using provided options
        /// </summary>
        /// <param name="optionsDelegate">The options to use for this client</param>
        public BitstampClient(ILoggerFactory? loggerFactory = null, Action<BitstampRestOptions>? optionsDelegate = null) : base(loggerFactory, "Bitstamp")
        {
            var options = BitstampRestOptions.Default.Copy();
            if(optionsDelegate != null)
            {
                optionsDelegate(options);
            }
            Initialize(options);

            Api = AddApiClient(new BitstampApiClient(_logger, options));
        }
        #endregion

        /// <summary>
        /// Set the default options to be used when creating new clients
        /// </summary>
        /// <param name="options"></param>
        public static void SetDefaultOptions(Action<BitstampRestOptions> optionsDelegate)
        {
            var options = BitstampRestOptions.Default.Copy();
            optionsDelegate(options);
            BitstampRestOptions.Default = options;
        }

        /// <summary>
        /// Set the API credentials for this client. All Api clients in this client will use the new credentials, regardless of earlier set options.
        /// </summary>
        /// <param name="credentials">The credentials to set</param>
        public void SetApiCredentials(ApiCredentials credentials)
        {
            Api.SetApiCredentials(credentials);
        }
    }
}