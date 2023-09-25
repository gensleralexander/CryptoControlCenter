using System;
using System.Collections.Generic;
using System.Net.Http;
using Bitstamp.Net.Enums;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Options;

namespace Bitstamp.Net.Objects
{
    public class BitstampRestOptions : RestExchangeOptions<BitstampEnvironment>
    {
        /// <summary>
        /// Interval for refreshing the auto timestamp calculation
        /// </summary>
        public TimeSpan AutoTimestampRecalculationInterval { get; set; } = TimeSpan.FromHours(3);

        /// <summary>
        /// A manual offset for the timestamp. Should only be used if AutoTimestamp and regular time synchronization on the OS is not reliable enough
        /// </summary>
        public TimeSpan TimestampOffset { get; set; } = TimeSpan.Zero;

        /// <summary>
        /// The default receive window for requests
        /// </summary>
        public TimeSpan ReceiveWindow { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Default Options for the spot client
        /// </summary>
        public static BitstampRestOptions Default { get; set; } = new BitstampRestOptions()
        {
            Environment = BitstampEnvironment.Live,
            AutoTimestamp = true,
            OutputOriginalData = true,
        };


        public BitstampRestApiOptions SpotOptions { get; private set; } = new BitstampRestApiOptions()
        {
            RateLimiters = new List<IRateLimiter>
                {
                    new RateLimiter()
                        .AddPartialEndpointLimit("/api/", 8000, TimeSpan.FromMinutes(10))
                        .AddEndpointLimit("/api/v3/order", 8000, TimeSpan.FromMinutes(10), HttpMethod.Post, true)
                }
        };

        internal BitstampRestOptions Copy()
        {
            var options = Copy<BitstampRestOptions>();
            options.ReceiveWindow = ReceiveWindow;
            options.SpotOptions = SpotOptions.Copy();
            return options;
        }
    }

    /// <summary>
    /// Bitstamp Api Client Options
    /// </summary>
    public class BitstampRestApiOptions : RestApiOptions
    {
        /// <summary>
        /// A manual offset for the timestamp. Should only be used if AutoTimestamp and regular time synchronization on the OS is not reliable enough
        /// </summary>
        public TimeSpan TimestampOffset { get; set; } = TimeSpan.Zero;

        /// <summary>
        /// Whether to check the trade rules when placing new orders and what to do if the trade isn't valid
        /// </summary>
        public TradeRulesBehaviour TradeRulesBehaviour { get; set; } = TradeRulesBehaviour.None;
        /// <summary>
        /// How often the trade rules should be updated. Only used when TradeRulesBehaviour is not None
        /// </summary>
        public TimeSpan TradeRulesUpdateInterval { get; set; } = TimeSpan.FromMinutes(60);

        /// <summary>
        /// ctor
        /// </summary>
        public BitstampRestApiOptions()
        {
        }

        internal BitstampRestApiOptions Copy()
        {
            var result = base.Copy<BitstampRestApiOptions>();
            result.TimestampOffset = TimestampOffset;
            result.TradeRulesBehaviour = TradeRulesBehaviour;
            result.TradeRulesUpdateInterval = TradeRulesUpdateInterval;
            return result;
        }
    }

    /// <summary>
    /// Bitstamp socket client options
    /// </summary>
    public class BitstampSocketOptions : SocketExchangeOptions<BitstampEnvironment>
    {
        /// <summary>
        /// Default options for the socket client
        /// </summary>
        public static BitstampSocketOptions Default { get; set; } = new BitstampSocketOptions()
        {
            Environment = BitstampEnvironment.Live,
            SocketSubscriptionsCombineTarget = 10
        };
    }
}
