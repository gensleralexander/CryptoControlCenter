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
            _baseClient.Options.SpotOptions.OutputOriginalData = true;
        }

        #region UserTransactions
        /// <inheritdoc />
        public async Task<WebCallResult<IEnumerable<BitstampUserTransaction>>> GetUserTransactionsAsync(string tradingpair = null, int offset = 0, int limit = 1000, SortingOrder sort = SortingOrder.Descending, DateTime? since_timestamp = null, long? since_id = null, CancellationToken ct = default)
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
            //requires Original Data
            if (result.Success)
            {
                List<BitstampUserTransaction> output = new List<BitstampUserTransaction>();
                CultureInfo culture = new CultureInfo("en-US");

                List<Dictionary<string, dynamic>> transactions = JsonConvert.DeserializeObject<List<Dictionary<string, dynamic>>>(result.OriginalData);
                foreach (Dictionary<string, dynamic> transaction in transactions)
                {
                    try
                    {
                        BitstampUserTransaction t = new BitstampUserTransaction();
                        foreach (KeyValuePair<string, dynamic> pair in transaction)
                        {
                            switch (pair.Key.ToLower())
                            {
                                case "id":
                                    t.Id = long.Parse(pair.Value.ToString(), culture);
                                    break;
                                case "datetime":
                                    t.TransactionTime = DateTime.Parse(pair.Value.ToString(), culture);
                                    break;
                                case "type":
                                    t.Type = (UserTransactionType)int.Parse(pair.Value.ToString(), culture);
                                    break;
                                case "fee":
                                    t.FeeAmount = decimal.Parse(pair.Value.ToString(), culture);
                                    break;
                                case "order_id":
                                    t.OrderId = long.Parse(pair.Value.ToString(), culture);
                                    break;
                                default:
                                    if (pair.Key.Contains("_"))
                                    {
                                        t.ExchangeRate = decimal.Parse(pair.Value.ToString(), culture);
                                        t.tradingPair = pair.Key;
                                    }
                                    else
                                    {
                                        decimal value = decimal.Parse(pair.Value.ToString(), culture);
                                        if (value > 0.0m)
                                        {
                                            t.ToAmount = value;
                                            t.ToAsset = pair.Key.ToUpper();
                                        }
                                        else if (value < 0.0m)
                                        {
                                            t.FromAmount = value * -1.0m;
                                            t.FromAsset = pair.Key.ToUpper();
                                        }
                                    }
                                    break;
                            }
                        }
                        output.Add(t);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
                //end of dynamic area
                return result.As(output as IEnumerable<BitstampUserTransaction>); //result.As(result.Data);
            }
            //Errors gets normally handled
            else return result.As(result.Data);
        }
        #endregion

        private IEnumerable<BitstampUserTransaction> GetDynamicTransactions(string originalData)
        {
            IEnumerable<BitstampUserTransaction> output = new List<BitstampUserTransaction>();


            return output;
        }


        #region Crypto-Transactions
        /// <inheritdoc />
        public async Task<WebCallResult<BitstampCryptoTransactions>> GetCryptoDepositsAndWithdrawalsAsync(int offset = 0, int limit = 1000, bool includeRippleIOUs = true, CancellationToken ct = default)
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
