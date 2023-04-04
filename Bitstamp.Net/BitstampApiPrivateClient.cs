using Bitstamp.Net.Enums;
using Bitstamp.Net.Objects;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using Bitstamp.Net.Interfaces.SubClients;
using Newtonsoft.Json;
using System.Dynamic;
using Microsoft.CSharp;
using Newtonsoft.Json.Linq;

namespace Bitstamp.Net
{
    public sealed class BitstampApiPrivateClient : IBitstampApiPrivateClient
    {
        //Private endpoints
        private const string balanceEndpoint = "balance";
        private const string userTransactionsEndpoint = "user_transactions";
        private const string cryptoTransactionsEndpoint = "crypto-transactions";
        private const string openOrdersEndpoint = "open_orders/all";
        private const string orderStatusEndpoint = "order_status";
        private const string cancelOrderEndpoint = "cancel_order";
        private const string cancelAllOrdersEndpoint = "cancel_all_orders";
        private const string buyLimitOrderEndpoint = "buy";
        private const string buyMarketOrderEndpoint = "buy/market";
        private const string buyInstantOrderEndpoint = "buy/instant";
        private const string sellLimitOrderEndpoint = "sell";
        private const string sellMarketOrderEndpoint = "sell/market";
        private const string sellInstantOrderEndpoint = "sell/instant";
        private const string websocketsTokenEndpoint = "websockets_token";
        // General
        private const string api = "api";
        private const string version = "2";

        private readonly BitstampApiClient _baseClient;

        internal BitstampApiPrivateClient(BitstampApiClient baseClient)
        {
            _baseClient = baseClient;
            _baseClient.Options.SpotApiOptions.OutputOriginalData = true;
        }

        #region UserTransactions
        /// <summary>
        /// Queries the UserTransactions
        /// </summary>
        /// <param name="tradingpair">(Optional) A specific TradingPair</param>
        /// <param name="offset">Skip that many transactions before returning results (default: 0, maximum: 200000). If you need to export older history contact support OR use combination of limit and since_id parameters</param>
        /// <param name="limit">Limit result to that many transactions (default: 100; maximum: 1000).</param>
        /// <param name="sort">Sorting by date and time: ascending; descending (default: descending).</param>
        /// <param name="since_timestamp">Show only transactions from unix timestamp (for max 30 days old).</param>
        /// <param name="since_id">	Show only transactions from specified transaction id. If since_id parameter is used, limit parameter is set to 1000.</param>
        /// <param name="ct">Task Cancellation Token</param>
        /// <returns>IEnumerable of UserTransactions</returns>
        public async Task<WebCallResult<IEnumerable<BitstampUserTransaction>>> GetUserTransactionsAsync(string tradingpair = null, int offset = 0, int limit = 100, SortingOrder sort = SortingOrder.Descending, DateTime? since_timestamp = null, long? since_id = null, CancellationToken ct = default)
        {
            limit.ValidateIntBetween(nameof(limit), 1, 1000);
            offset.ValidateIntBetween(nameof(offset), 0, 200000);
            string sortString = "desc";
            if (sort == SortingOrder.Ascending)
            {
                sortString = "asc";
            }
            var parameters = new Dictionary<string, object> {
                { "offset", offset },
                { "limit", limit },
                { "sort", sortString }
            };
            parameters.AddOptionalParameter("since_timestamp", since_timestamp != null ? BitstampApiClient.ToUnixTimestamp(since_timestamp.Value).ToString(CultureInfo.InvariantCulture) : null);
            parameters.AddOptionalParameter("since_id", since_id != null ? since_id : null);

            var result = await _baseClient.SendRequestInternal<IEnumerable<BitstampUserTransaction>>(_baseClient.GetUrl(userTransactionsEndpoint, api, version, tradingpair), HttpMethod.Post, ct, parameters, true, true, HttpMethodParameterPosition.InBody).ConfigureAwait(false);
            //This area is implemented to create dynamic BitstampUserTransactionTypes, as the original ones need regular maintenance (everytime, Bitstamp adds a new coin, the HTTP answer is different
            //needs to be non-async method, otherwise compiler issues
            List<BitstampUserTransaction> output = new List<BitstampUserTransaction>();
            CultureInfo culture = new CultureInfo("en-US");

            List<Dictionary<string, dynamic>> transactions = JsonConvert.DeserializeObject<List<Dictionary<string, dynamic>>>(result.OriginalData);
            foreach (Dictionary<string, dynamic> transaction in transactions)
            {
                try
                {
                    BitstampUserTransaction t = new BitstampUserTransaction();
                    t.Id = long.Parse(transaction["id"].ToString(), culture);
                    t.TransactionTime = DateTime.Parse(transaction["datetime"].ToString(), culture);
                    t.FeeAmount = decimal.Parse(transaction["fee"].ToString(), culture);
                    try
                    {
                        //Order IDs are only in market trades included
                        t.OrderId = long.Parse(transaction["order_id"].ToString(), culture);
                    }
                    catch { }
                    t.Type = (UserTransactionType)int.Parse(transaction["type"].ToString(), culture);
                    foreach (KeyValuePair<string, dynamic> pair in transaction)
                    {
                        try
                        {
                            decimal value = decimal.Parse(pair.Value.ToString(), culture);
                            if (value > 0.0m)
                            {
                                if (pair.Key.ToLower() != "order_id")
                                {
                                    if (pair.Key.Contains("_"))
                                    {
                                        t.ExchangeRate = value;
                                        t.tradingPair = pair.Key;
                                    }
                                    else
                                    {
                                        t.ToAmount = value;
                                        t.ToAsset = pair.Key.ToUpper();
                                    }
                                }
                            }
                            else if (value < 0.0m)
                            {
                                t.FromAmount = value * -1.0m;
                                t.FromAsset = pair.Key.ToUpper();
                            }
                        }
                        catch { }
                    }
                    output.Add(t);
                }
                catch { }
            }

            //end of dynamic area
            return result.As(output as IEnumerable<BitstampUserTransaction>); //result.As(result.Data);
        }
        #endregion

        private IEnumerable<BitstampUserTransaction> GetDynamicTransactions(string originalData)
        {
            IEnumerable<BitstampUserTransaction> output = new List<BitstampUserTransaction>();


            return output;
        }


        #region Crypto-Transactions
        /// <summary>
        /// Queries the Crypto Withdrawals and Deposits
        /// </summary>
        /// <param name="offset">Skip that many transactions before returning results (default: 0, maximum: 200000).</param>
        /// <param name="limit">Limit result to that many transactions (default: 100; maximum: 1000).</param>
        /// <param name="includeRippleIOUs">Shows also ripple IOU transactions</param>
        /// <param name="ct">Task Cancellation Token</param>
        /// <returns>IEnumerable of UserTransactions</returns>
        public async Task<WebCallResult<BitstampCryptoTransactions>> GetCryptoDepositsAndWithdrawalsAsync(int offset = 0, int limit = 100, bool includeRippleIOUs = true, CancellationToken ct = default)
        {
            limit.ValidateIntBetween(nameof(limit), 1, 1000);
            offset.ValidateIntBetween(nameof(offset), 0, 200000);

            var parameters = new Dictionary<string, object> {
                { "offset", offset },
                { "limit", limit }
            };
            parameters.AddOptionalParameter("include_ious", includeRippleIOUs);
            var result = await _baseClient.SendRequestInternal<BitstampCryptoTransactions>(_baseClient.GetUrl(cryptoTransactionsEndpoint, api, version, null), HttpMethod.Post, ct, parameters, true, true, HttpMethodParameterPosition.InBody).ConfigureAwait(false);
            return result.As(result.Data);
        }
        #endregion


    }
}
