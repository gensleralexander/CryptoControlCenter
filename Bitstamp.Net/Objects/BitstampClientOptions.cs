using System;
using System.Collections.Generic;
using System.Net.Http;
using Bitstamp.Net.Enums;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;

namespace Bitstamp.Net.Objects
{
    public class BitstampClientOptions : ClientOptions
    {
        /// <summary>
        /// Whether or not to automatically sync the local time with the server time
        /// </summary>
        public bool AutoTimestamp { get; set; } = true;

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
        public static BitstampClientOptions Default { get; set; } = new BitstampClientOptions();


        private BitstampApiClientOptions _spotApiOptions = new BitstampApiClientOptions(BitstampApiAddresses.Default.RestClientAddress)
        {
            AutoTimestamp = true,
            RateLimiters = new List<IRateLimiter>
                {
                    new RateLimiter()
                        .AddPartialEndpointLimit("/api/", 8000, TimeSpan.FromMinutes(10))
                        .AddEndpointLimit("/api/v3/order", 8000, TimeSpan.FromMinutes(10), HttpMethod.Post, true)
                }
        };
        /// <summary>
        /// Spot API options
        /// </summary>
        public BitstampApiClientOptions SpotApiOptions
        {
            get => _spotApiOptions;
            set => _spotApiOptions = new BitstampApiClientOptions(_spotApiOptions, value);
        }

        /// <summary>
        /// Constructor with default endpoints
        /// </summary>
        public BitstampClientOptions() : this(Default)
        {
        }

        /// <summary>
        /// Constructor with default endpoints
        /// </summary>
        internal BitstampClientOptions(BitstampClientOptions baseOn) : base(baseOn)
        {
            if (baseOn == null)
                return;

            ReceiveWindow = baseOn.ReceiveWindow;
            AutoTimestamp = baseOn.AutoTimestamp;
        }
    }

    /// <summary>
    /// Bitstamp Api Client Options
    /// </summary>
    public class BitstampApiClientOptions : RestApiClientOptions
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
        public BitstampApiClientOptions()
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="baseAddress"></param>
        internal BitstampApiClientOptions(string baseAddress) : base(baseAddress)
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="baseOn"></param>
        /// <param name="newValues"></param>
        internal BitstampApiClientOptions(BitstampApiClientOptions baseOn, BitstampApiClientOptions? newValues) : base(baseOn, newValues)
        {
            TimestampOffset = newValues?.TimestampOffset ?? baseOn.TimestampOffset;
            TradeRulesBehaviour = newValues?.TradeRulesBehaviour ?? baseOn.TradeRulesBehaviour;
            TradeRulesUpdateInterval = newValues?.TradeRulesUpdateInterval ?? baseOn.TradeRulesUpdateInterval;
        }
    }

    /// <summary>
    /// Bitstamp socket client options
    /// </summary>
    public class BitstampSocketClientOptions : ClientOptions
    {
        /// <summary>
        /// Default options for the socket client
        /// </summary>
        public static BitstampSocketClientOptions Default { get;set; } = new BitstampSocketClientOptions();

        /// <summary>
        /// ctor
        /// </summary>
        public BitstampSocketClientOptions() : this(Default)
        {
        }

        internal BitstampSocketClientOptions(BitstampSocketClientOptions baseOn) : base(baseOn)
        {
            if (baseOn == null)
                return;

            
        }
    }
}
