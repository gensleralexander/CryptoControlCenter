using Bitstamp.Net.Interfaces.SubClients;
using Bitstamp.Net.Objects;
using Bitstamp.Net.Objects.Helper;
using CryptoExchange.Net;
using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Bitstamp.Net
{
    /// <summary>
    /// Spot Client for public endpoints
    /// </summary>
    public sealed class BitstampApiPublicClient : IBitstampApiPublicClient
    {
        //Public endpoints
        private const string ohlcEndpoint = "ohlc";
        private const string tickerEndpoint = "ticker";
        private const string tickerHourlyEndpoint = "ticker_hour";
        private const string orderBookEndpoint = "order_book";
        private const string transactionsEndpoint = "transactions";
        private const string tradingPairsInfoEndpoint = "trading-pairs-info";
        //General
        private const string api = "api";
        private const string version = "2";

        private readonly BitstampApiClient _baseClient;

        internal BitstampApiPublicClient(BitstampApiClient baseClient)
        {
            _baseClient = baseClient;
        }

        #region Kline/Candlestick Data
        /// <summary>
        /// Get candlestick data for the provided symbol
        /// </summary>
        /// <param name="pair">The trading pair to get the data for</param>
        /// <param name="interval">The candlestick timespan</param>
        /// <param name="startTime">Start time to get candlestick data</param>
        /// <param name="endTime">End time to get candlestick data</param>
        /// <param name="limit">Max number of results</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>The candlestick data for the provided symbol</returns>
        public async Task<WebCallResult<BitstampData<OHLCData>>> GetOHLCDataAsync(string pair, int step, int limit, DateTime? startTime = null, DateTime? endTime = null, CancellationToken ct = default)
        {
            limit.ValidateIntBetween(nameof(limit), 1, 1000);
            var parameters = new Dictionary<string, object> {
                { "step", step },
                { "limit", limit }
            };
            parameters.AddOptionalParameter("start", startTime != null ? BitstampApiClient.ToUnixTimestamp(startTime.Value).ToString(CultureInfo.InvariantCulture) : null);
            parameters.AddOptionalParameter("end", endTime != null ? BitstampApiClient.ToUnixTimestamp(endTime.Value).ToString(CultureInfo.InvariantCulture) : null);

            var result = await _baseClient.SendRequestInternal<BitstampData<OHLCData>>(_baseClient.GetUrl(ohlcEndpoint, api, version, pair), HttpMethod.Get, ct, parameters).ConfigureAwait(false);
            return result.As(result.Data);
        }


        #endregion

        #region TradingPair Info
        /// <summary>
        /// Queries the Trading Pairs for Bitstamp
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Collection of trading pairs</returns>
        public async Task<WebCallResult<IEnumerable<BitstampTradingPairInfo>>> GetTradingPairInfo(CancellationToken ct = default)
        {
            var result = await _baseClient.SendRequestInternal<IEnumerable<BitstampTradingPairInfo>>(_baseClient.GetUrl(tradingPairsInfoEndpoint, api, version, null), HttpMethod.Get, ct).ConfigureAwait(false);
            return result.As(result.Data);
        }

        #endregion

        #region Symbol Price Ticker

        /// <summary>
        /// Gets the ticker for specific pair
        /// </summary>
        /// <param name="pair">The pari to get the ticker for</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Ticker of pair</returns>
        public async Task<WebCallResult<BitstampTicker>> GetTickerAsync(string pair, CancellationToken ct = default)
        {
                return await _baseClient.SendRequestInternal<BitstampTicker>(_baseClient.GetUrl(tickerEndpoint, api, version, pair), HttpMethod.Get, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Get a list of tickers of all pairs
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        /// <returns>List of tickers</returns>
        public async Task<WebCallResult<IEnumerable<BitstampTicker>>> GetAllTickersAsync(CancellationToken ct = default)
        {
            return await _baseClient.SendRequestInternal<IEnumerable<BitstampTicker>>(_baseClient.GetUrl(tickerEndpoint, api, version, null), HttpMethod.Get, ct).ConfigureAwait(false);
        }

        #endregion

        #region Order Book
        ///// <summary>
        ///// Gets the order book for the provided symbol
        ///// </summary>
        ///// <param name="symbol">The symbol to get the order book for</param>
        ///// <param name="limit">Max number of results</param>
        ///// <param name="ct">Cancellation token</param>
        ///// <returns>The order book for the symbol</returns>
        //public async Task<WebCallResult<OrderBook>> GetOrderBookAsync(string symbol, int? limit = null, CancellationToken ct = default)
        //{
        //    throw new NotImplementedException();
        //    //symbol.ValidateBitstampSymbol();
        //    //limit?.ValidateIntValues(nameof(limit), 5, 10, 20, 50, 100, 500, 1000, 5000);
        //    //var parameters = new Dictionary<string, object> { { "symbol", symbol } };
        //    //parameters.AddOptionalParameter("limit", limit?.ToString(CultureInfo.InvariantCulture));
        //    //var result = await _baseClient.SendRequestInternal<BitstampOrderBook>(_baseClient.GetUrlSpot(orderBookEndpoint, api, publicVersion), HttpMethod.Get, ct, parameters).ConfigureAwait(false);
        //    //if (result)
        //    //    result.Data.Symbol = symbol;
        //    //return result;
        //}
        #endregion

        #region Recent Trades List

        ///// <summary>
        ///// Gets the recent trades for a symbol
        ///// </summary>
        ///// <param name="symbol">The symbol to get recent trades for</param>
        ///// <param name="limit">Result limit</param>
        ///// <param name="ct">Cancellation token</param>
        ///// <returns>List of recent trades</returns>
        //public async Task<WebCallResult<IEnumerable<IBitstampRecentTrade>>> GetRecentTradeHistoryAsync(string symbol, int? limit = null, CancellationToken ct = default)
        //{
        //    symbol.ValidateBitstampSymbol();
        //    limit?.ValidateIntBetween(nameof(limit), 1, 1000);

        //    var parameters = new Dictionary<string, object> { { "symbol", symbol } };
        //    parameters.AddOptionalParameter("limit", limit?.ToString(CultureInfo.InvariantCulture));
        //    var result = await _baseClient.SendRequestInternal<IEnumerable<BitstampRecentTradeQuote>>(_baseClient.GetUrlSpot(transactionsEndpoint, api, publicVersion), HttpMethod.Get, ct, parameters).ConfigureAwait(false);
        //    return result.As<IEnumerable<IBitstampRecentTrade>>(result.Data);
        //}

        #endregion

        #region Old Trade Lookup

        ///// <summary>
        ///// Gets the historical  trades for a symbol
        ///// </summary>
        ///// <param name="symbol">The symbol to get recent trades for</param>
        ///// <param name="limit">Result limit</param>
        ///// <param name="fromId">From which trade id on results should be retrieved</param>
        ///// <param name="ct">Cancellation token</param>
        ///// <returns>List of recent trades</returns>
        //public async Task<WebCallResult<IEnumerable<IBitstampRecentTrade>>> GetTradeHistoryAsync(string symbol, int? limit = null, long? fromId = null, CancellationToken ct = default)
        //{
        //    symbol.ValidateBitstampSymbol();
        //    limit?.ValidateIntBetween(nameof(limit), 1, 1000);
        //    var parameters = new Dictionary<string, object> { { "symbol", symbol } };
        //    parameters.AddOptionalParameter("limit", limit?.ToString(CultureInfo.InvariantCulture));
        //    parameters.AddOptionalParameter("fromId", fromId?.ToString(CultureInfo.InvariantCulture));

        //    var result = await _baseClient.SendRequestInternal<IEnumerable<BitstampRecentTradeQuote>>(_baseClient.GetUrlSpot(historicalTradesEndpoint, api, publicVersion), HttpMethod.Get, ct, parameters).ConfigureAwait(false);
        //    return result.As<IEnumerable<IBitstampRecentTrade>>(result.Data);
        //}

        #endregion

        #region Current Average Price

        ///// <summary>
        ///// Gets current average price for a symbol
        ///// </summary>
        ///// <param name="symbol">The symbol to get the data for</param>
        ///// <param name="ct">Cancellation token</param>
        ///// <returns></returns>
        //public async Task<WebCallResult<BitstampAveragePrice>> GetCurrentAvgPriceAsync(string symbol, CancellationToken ct = default)
        //{
        //    symbol.ValidateBitstampSymbol();
        //    var parameters = new Dictionary<string, object> { { "symbol", symbol } };

        //    return await _baseClient.SendRequestInternal<BitstampAveragePrice>(_baseClient.GetUrlSpot(averagePriceEndpoint, api, publicVersion), HttpMethod.Get, ct, parameters).ConfigureAwait(false);
        //}

        #endregion

        #region 24hr Ticker Price Change Statistics

        ///// <summary>
        ///// Get data regarding the last 24 hours for the provided symbol
        ///// </summary>
        ///// <param name="symbol">The symbol to get the data for</param>
        ///// <param name="ct">Cancellation token</param>
        ///// <returns>Data over the last 24 hours</returns>
        //public async Task<WebCallResult<IBitstampTick>> GetTickerAsync(string symbol, CancellationToken ct = default)
        //{
        //    symbol.ValidateBitstampSymbol();
        //    var parameters = new Dictionary<string, object> { { "symbol", symbol } };

        //    var result = await _baseClient.SendRequestInternal<Bitstamp24HPrice>(_baseClient.GetUrlSpot(tickerEndpoint, api, publicVersion), HttpMethod.Get, ct, parameters).ConfigureAwait(false);
        //    return result.As<IBitstampTick>(result.Data);
        //}

        ///// <summary>
        ///// Get data regarding the last 24 hours for all symbols
        ///// </summary>
        ///// <param name="ct">Cancellation token</param>
        ///// <returns>List of data over the last 24 hours</returns>
        //public async Task<WebCallResult<IEnumerable<IBitstampTick>>> GetTickersAsync(CancellationToken ct = default)
        //{
        //    var result = await _baseClient.SendRequestInternal<IEnumerable<Bitstamp24HPrice>>(_baseClient.GetUrlSpot(tickerEndpoint, api, publicVersion), HttpMethod.Get, ct).ConfigureAwait(false);
        //    return result.As<IEnumerable<IBitstampTick>>(result.Data);
        //}

        #endregion

        #region Symbol Price Ticker

        ///// <summary>
        ///// Gets the price of a symbol
        ///// </summary>
        ///// <param name="symbol">The symbol to get the price for</param>
        ///// <param name="ct">Cancellation token</param>
        ///// <returns>Price of symbol</returns>
        //public async Task<WebCallResult<BitstampPrice>> GetPriceAsync(string symbol, CancellationToken ct = default)
        //{
        //    symbol.ValidateBitstampSymbol();
        //    var parameters = new Dictionary<string, object>
        //    {
        //        { "symbol", symbol }
        //    };

        //    return await _baseClient.SendRequestInternal<BitstampPrice>(_baseClient.GetUrlSpot(tickerHourlyEndpoint, api, publicVersion), HttpMethod.Get, ct, parameters).ConfigureAwait(false);
        //}

        ///// <summary>
        ///// Get a list of the prices of all symbols
        ///// </summary>
        ///// <param name="ct">Cancellation token</param>
        ///// <returns>List of prices</returns>
        //public async Task<WebCallResult<IEnumerable<BitstampPrice>>> GetPricesAsync(CancellationToken ct = default)
        //{
        //    return await _baseClient.SendRequestInternal<IEnumerable<BitstampPrice>>(_baseClient.GetUrlSpot(tickerHourlyEndpoint, api, publicVersion), HttpMethod.Get, ct).ConfigureAwait(false);
        //}

        #endregion

        #region Symbol Order Book Ticker

        ///// <summary>
        ///// Gets the best price/quantity on the order book for a symbol.
        ///// </summary>
        ///// <param name="symbol">Symbol to get book price for</param>
        ///// <param name="ct">Cancellation token</param>
        ///// <returns>List of book prices</returns>
        //public async Task<WebCallResult<BitstampBookPrice>> GetBookPriceAsync(string symbol, CancellationToken ct = default)
        //{
        //    symbol.ValidateBitstampSymbol();
        //    var parameters = new Dictionary<string, object> { { "symbol", symbol } };

        //    return await _baseClient.SendRequestInternal<BitstampBookPrice>(_baseClient.GetUrlSpot(bookPricesEndpoint, api, publicVersion), HttpMethod.Get, ct, parameters).ConfigureAwait(false);
        //}

        ///// <summary>
        ///// Gets the best price/quantity on the order book for all symbols.
        ///// </summary>
        ///// <param name="ct">Cancellation token</param>
        ///// <returns>List of book prices</returns>
        //public async Task<WebCallResult<IEnumerable<BitstampBookPrice>>> GetAllBookPricesAsync(CancellationToken ct = default)
        //{
        //    return await _baseClient.SendRequestInternal<IEnumerable<BitstampBookPrice>>(_baseClient.GetUrlSpot(bookPricesEndpoint, api, publicVersion), HttpMethod.Get, ct).ConfigureAwait(false);
        //}

        #endregion
    }
}
