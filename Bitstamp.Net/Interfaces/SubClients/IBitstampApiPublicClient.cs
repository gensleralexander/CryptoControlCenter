using Bitstamp.Net.Objects;
using Bitstamp.Net.Objects.Helper;
using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bitstamp.Net.Interfaces.SubClients
{
    /// <summary>
    /// Spot interface for public endpoints
    /// </summary>
    public interface IBitstampApiPublicClient
    {
        /// <summary>
        /// Get candlestick data for the provided symbol
        /// </summary>
        /// <param name="tradingpair">The trading pair to get the data for</param>
        /// <param name="step">The candlestick timespan in seconds</param>
        /// <param name="startTime">Start time to get candlestick data</param>
        /// <param name="endTime">End time to get candlestick data</param>
        /// <param name="limit">Max number of results</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>The candlestick data for the provided symbol</returns>
        Task<WebCallResult<BitstampData<OHLCData>>> GetOHLCDataAsync(string tradingpair, int step, int limit, DateTime? startTime = null, DateTime? endTime = null, CancellationToken ct = default);
        /// <summary>
        /// Queries the Trading Pairs for Bitstamp
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Collection of trading pairs</returns>
        Task<WebCallResult<IEnumerable<BitstampTradingPairInfo>>> GetTradingPairInfo(CancellationToken ct = default);
        /// <summary>
        /// Gets the ticker for specific pair
        /// </summary>
        /// <param name="pair">The pari to get the ticker for</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Ticker of pair</returns>
        Task<WebCallResult<BitstampTicker>> GetTickerAsync(string pair, CancellationToken ct = default);
        /// <summary>
        /// Get a list of tickers of all pairs
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        /// <returns>List of tickers</returns>
        Task<WebCallResult<IEnumerable<BitstampTicker>>> GetAllTickersAsync(CancellationToken ct = default);
    }
}