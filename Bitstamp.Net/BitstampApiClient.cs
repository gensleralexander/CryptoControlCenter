using Bitstamp.Net.Interfaces.SubClients;
using Bitstamp.Net.Objects;
using CryptoExchange.Net;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Data;

namespace Bitstamp.Net
{
    public class BitstampApiClient : RestApiClient, IBitstampApiClient
    {
        #region fields 
        internal new readonly BitstampRestOptions Options;
        internal static double CalculatedTimeOffset;

        //internal BitstampTradingPairInfo? TradingPairInfo;
        internal DateTime? LastExchangeInfoUpdate;

        internal static TimeSyncState TimeSyncState = new TimeSyncState("Spot Api");
        #endregion

        #region Api clients
        /// <inheritdoc />
        public IBitstampApiPublicClient Public { get; }
        /// <inheritdoc />
        public IBitstampApiPrivateClient Private { get; }
        /// <inheritdoc />
        public string ExchangeName => "Bitstamp";
        #endregion

        /// <summary>
        /// Event triggered when an order is placed via this client. Only available for Spot orders
        /// </summary>
        public event Action<OrderId>? OnOrderPlaced;
        /// <summary>
        /// Event triggered when an order is canceled via this client. Note that this does not trigger when using CancelAllOrdersAsync. Only available for Spot orders
        /// </summary>
        public event Action<OrderId>? OnOrderCanceled;

        #region constructor/destructor
        internal BitstampApiClient(ILogger log, BitstampRestOptions options) : base(log, null, options.Environment.SpotRestAddress, options, options.SpotOptions)
        {
            options.SpotOptions.OutputOriginalData = true;
            Options = options;

            Public = new BitstampApiPublicClient(this);
            Private = new BitstampApiPrivateClient(this);

            requestBodyEmptyContent = string.Empty;
            requestBodyFormat = RequestBodyFormat.FormData;
            arraySerialization = ArrayParametersSerialization.MultipleValues;
        }
        #endregion

        /// <inheritdoc />
        protected override AuthenticationProvider CreateAuthenticationProvider(ApiCredentials credentials)
            => new BitstampAuthenticationProvider(credentials);

        #region Override ConstructRequest + WriteParamBody
        /// <summary>
        /// Creates a request object
        /// </summary>
        /// <param name="uri">The uri to send the request to</param>
        /// <param name="method">The method of the request</param>
        /// <param name="parameters">The parameters of the request</param>
        /// <param name="signed">Whether or not the request should be authenticated</param>
        /// <param name="parameterPosition">Where the parameters should be placed</param>
        /// <param name="arraySerialization">How array parameters should be serialized</param>
        /// <param name="requestId">Unique id of a request</param>
        /// <param name="additionalHeaders">Additional headers to send with the request</param>
        /// <returns></returns>
        protected override IRequest ConstructRequest(
            Uri uri,
            HttpMethod method,
            Dictionary<string, object>? parameters,
            bool signed,
            HttpMethodParameterPosition parameterPosition,
            ArrayParametersSerialization arraySerialization,
            RequestBodyFormat bodyFormat,
            int requestId,
            Dictionary<string, string>? additionalHeaders)
        {
            parameters ??= new Dictionary<string, object>();

            for (var i = 0; i < parameters.Count; i++)
            {
                var kvp = parameters.ElementAt(i);
                if (kvp.Value is Func<object> delegateValue)
                    parameters[kvp.Key] = delegateValue();
            }

            if (parameterPosition == HttpMethodParameterPosition.InUri)
            {
                foreach (var parameter in parameters)
                    uri = uri.AddQueryParmeter(parameter.Key, parameter.Value.ToString());
            }

            var headers = new Dictionary<string, string>();
            var uriParameters = parameterPosition == HttpMethodParameterPosition.InUri ? new SortedDictionary<string, object>(parameters) : new SortedDictionary<string, object>();
            var bodyParameters = parameterPosition == HttpMethodParameterPosition.InBody ? new SortedDictionary<string, object>(parameters) : new SortedDictionary<string, object>();

            if (AuthenticationProvider != null)
                AuthenticationProvider.AuthenticateRequest(this, uri, method, parameters, signed, arraySerialization, parameterPosition, out uriParameters, out bodyParameters, out headers);

            // Sanity check
            foreach (var param in parameters)
            {
                if (!uriParameters.ContainsKey(param.Key) && !bodyParameters.ContainsKey(param.Key))
                {
                    throw new Exception($"Missing parameter {param.Key} after authentication processing. AuthenticationProvider implementation " +
                        $"should return provided parameters in either the uri or body parameters output");
                }
            }

            var request = RequestFactory.Create(method, uri, requestId);
            request.Accept = Constants.JsonContentHeader;


            foreach (var header in headers)
                request.AddHeader(header.Key, header.Value);

            if (additionalHeaders != null)
            {
                foreach (var header in additionalHeaders)
                    request.AddHeader(header.Key, header.Value);
            }

            if (StandardRequestHeaders != null)
            {
                foreach (var header in StandardRequestHeaders)
                    // Only add it if it isn't overwritten
                    if (additionalHeaders?.ContainsKey(header.Key) != true)
                        request.AddHeader(header.Key, header.Value);
            }

            if (parameterPosition == HttpMethodParameterPosition.InBody)
            {
                var contentType = requestBodyFormat == RequestBodyFormat.Json ? Constants.JsonContentHeader : Constants.FormContentHeader;
                if (bodyParameters.Any())
                    WriteParamBody(request, bodyParameters, contentType);
                else request.SetContent(requestBodyEmptyContent, contentType);
            }

            return request;
        }

        /// <summary>
        /// Writes the parameters of the request to the request object body
        /// </summary>
        /// <param name="request">The request to set the parameters on</param>
        /// <param name="parameters">The parameters to set</param>
        /// <param name="contentType">The content type of the data</param>
        protected override void WriteParamBody(IRequest request, SortedDictionary<string, object> parameters, string contentType)
        {
            if (requestBodyFormat == RequestBodyFormat.Json)
            {
                // Write the parameters as json in the body
                var stringData = JsonConvert.SerializeObject(parameters.OrderBy(p => p.Key).ToDictionary(p => p.Key, p => p.Value));
                request.SetContent(stringData, contentType);
            }
            else if (requestBodyFormat == RequestBodyFormat.FormData)
            {
                // Write the parameters as form data in the body
                var formData = HttpUtility.ParseQueryString(string.Empty);
                foreach (var kvp in parameters.OrderBy(p => p.Key))
                {
                    if (kvp.Value.GetType().IsArray)
                    {
                        var array = (Array)kvp.Value;
                        foreach (var value in array)
                            formData.Add(kvp.Key, value.ToString());
                    }
                    else
                        formData.Add(kvp.Key, kvp.Value.ToString());
                }
                var stringData = formData.ToString();
                request.SetContent(stringData, contentType);
                var field = request.GetType().GetField("_request", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetField);
                //If exception after Library Update occurs here, check JKorfs Cryptoexchange Library Request.cs for changed field name _request
                var requestMsg = (HttpRequestMessage)field.GetValue(request);
                requestMsg.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
            }
        }
        #endregion


        #region helpers
        /// <inheritdoc />
        protected override async Task<ServerError?> TryParseErrorAsync(JToken error)
        {
            if (!error.HasValues)
                return new ServerError(error.ToString());

            if (error["message"] == null && error["code"] == null)
                return new ServerError(error.ToString());

            if (error["message"] != null && error["code"] == null)
                return new ServerError((string)error["msg"]!);

            ServerError err;
            try
            {
                err = new ServerError((string)error["reason"]!, (string)error["code"]!);
            }
            catch (Exception)
            {
                err = new ServerError((int)error["code"]!, (string)error["message"]!);
            }

            return err;
        }


        internal Error ParseErrorResponseInternal(JToken error) => TryParseErrorAsync(error).Result;

        internal Uri GetUrl(string endpoint, string api, string? version = null, string? pair = null)
        {
            var result = $"{BaseAddress}/{api}/";

            if (!string.IsNullOrEmpty(version))
                result += $"v{version}/";

            result += $"{endpoint}/";

            if (!string.IsNullOrEmpty(pair))
                result += $"{pair}/";
            return new Uri(result);
        }

        internal static long ToUnixTimestamp(DateTime time) //Bitstamp uses only the seconds of Unix Timestamp
        {
            return (long)(time - new DateTime(1970, 1, 1)).TotalSeconds;
        }

        internal string GetTimestamp()
        {
            var offset = Options.AutoTimestamp ? CalculatedTimeOffset : 0;
            offset += Options.TimestampOffset.TotalMilliseconds;
            return ToUnixTimestamp(DateTime.UtcNow.AddMilliseconds(offset)).ToString(CultureInfo.InvariantCulture);
        }

        internal async Task<WebCallResult<T>> SendRequestInternal<T>(Uri uri, HttpMethod method, CancellationToken cancellationToken, Dictionary<string, object>? parameters = null, bool signed = false, bool checkResult = true, HttpMethodParameterPosition? postPosition = null, ArrayParametersSerialization? arraySerialization = null) where T : class
        {
            var result = await SendRequestAsync<T>(uri, method, cancellationToken, parameters, signed, RequestBodyFormat.FormData, postPosition, arraySerialization, 1, ignoreRatelimit: false).ConfigureAwait(false);
            if (!result && result.Error!.Code == -1021 && (Options.SpotOptions.AutoTimestamp ?? ClientOptions.AutoTimestamp))
            {
                //log.Write(LogLevel.Debug, "Received Invalid Timestamp error, triggering new time sync");
                TimeSyncState.LastSyncTime = DateTime.MinValue;
            }
            return result;
        }
        #endregion


        /// <inheritdoc />
        public string GetSymbolName(string baseAsset, string quoteAsset) =>
            (baseAsset + quoteAsset).ToUpper(CultureInfo.InvariantCulture);

        private static int GetStepIntervalFromTimespan(TimeSpan timeSpan)
        {
            if (timeSpan == TimeSpan.FromMinutes(1)) return 60;
            if (timeSpan == TimeSpan.FromMinutes(3)) return 180;
            if (timeSpan == TimeSpan.FromMinutes(5)) return 300;
            if (timeSpan == TimeSpan.FromMinutes(15)) return 900;
            if (timeSpan == TimeSpan.FromMinutes(30)) return 1800;
            if (timeSpan == TimeSpan.FromHours(1)) return 3600;
            if (timeSpan == TimeSpan.FromHours(2)) return 7200;
            if (timeSpan == TimeSpan.FromHours(4)) return 14400;
            if (timeSpan == TimeSpan.FromHours(6)) return 21600;
            if (timeSpan == TimeSpan.FromHours(12)) return 43200;
            if (timeSpan == TimeSpan.FromDays(1)) return 86400;
            if (timeSpan == TimeSpan.FromDays(3)) return 259200;

            throw new ArgumentException("Unsupported timespan for Bitstamp Klines");
        }

        #region Base Class Not Implemented Methods
        public override TimeSyncInfo GetTimeSyncInfo()
        {
            return new TimeSyncInfo(_logger, Options.SpotOptions.AutoTimestamp ?? ClientOptions.AutoTimestamp, Options.SpotOptions.TimestampRecalculationInterval ?? ClientOptions.TimestampRecalculationInterval, TimeSyncState);
        }

        public override TimeSpan? GetTimeOffset()
        {
            return TimeSyncState.TimeOffset;
        }

        protected override Task<WebCallResult<DateTime>> GetServerTimestampAsync()
        {
            return Task.Run(ServerTimeStamp);
        }

        private WebCallResult<DateTime> ServerTimeStamp()
        {
            return new WebCallResult<DateTime>(System.Net.HttpStatusCode.OK,
                null,
                new TimeSpan(0, 0, 0),
                null,
                string.Empty,
                null,
                null,
                null,
                null,
                null,
                DateTime.UtcNow,
                null);
        }
    }
    #endregion
}