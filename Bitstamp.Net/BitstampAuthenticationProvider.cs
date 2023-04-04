using Bitstamp.Net.Objects;
using CryptoExchange.Net;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Bitstamp.Net
{
    internal class BitstampAuthenticationProvider : AuthenticationProvider
    {
        private readonly BitstampNonceProvider _nonceProvider;
        private readonly object signLock = new object();
        private readonly HMACSHA256 encryptor;

        public BitstampAuthenticationProvider(ApiCredentials credentials, BitstampNonceProvider nonceProvider = null) : base(credentials)
        {
            if (credentials.Secret == null)
                throw new ArgumentException("No valid API credentials provided. Key/Secret needed.");

            encryptor = new HMACSHA256(Encoding.ASCII.GetBytes(credentials.Secret.GetString()));
            _nonceProvider = nonceProvider ?? new BitstampNonceProvider(); //BitstampNonceProvider does not implement INonceProvider since the Bitstamp Nonce is a Guid, not a long
        }

        public override void AuthenticateRequest(RestApiClient apiClient, Uri uri, HttpMethod method, Dictionary<string, object> parameters, bool auth, ArrayParametersSerialization arraySerialization, HttpMethodParameterPosition parameterPosition, out SortedDictionary<string, object> uriParameters, out SortedDictionary<string, object> bodyParameters, out Dictionary<string, string> headers)
        {
            uriParameters = parameterPosition == HttpMethodParameterPosition.InUri ? new SortedDictionary<string, object>(parameters) : new SortedDictionary<string, object>();
            bodyParameters = parameterPosition == HttpMethodParameterPosition.InBody ? new SortedDictionary<string, object>(parameters) : new SortedDictionary<string, object>();
            headers = new Dictionary<string, string>();

            if (!auth)
                return;

            if (Credentials.Key == null)
                throw new ArgumentException("No valid API credentials provided. Key/Secret needed.");

            string xauth = "BITSTAMP " + Credentials.Key.GetString();
            string httpVerb = method.Method;
            string contentType = Constants.FormContentHeader;
            string nonce = _nonceProvider.GetNonce().ToString("D");
            string timestamp = Math.Round((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds).ToString(CultureInfo.InvariantCulture);
            string signature;



            string signData;
            if (parameterPosition == HttpMethodParameterPosition.InUri)
            {
                signData = parameters.CreateParamString(true, arraySerialization);
            }
            else
            {
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
                signData = xauth + httpVerb + uri.AbsoluteUri.Replace("https://", string.Empty) + contentType + nonce + timestamp + "v2" + formData.ToString();
            }


            lock (signLock)
            {
                signature = ByteToString(encryptor.ComputeHash(Encoding.UTF8.GetBytes(signData)));
            }

            headers = new Dictionary<string, string>() {
                { "X-Auth", xauth },
                { "X-Auth-Signature", signature },
                { "X-Auth-Nonce", nonce},
                { "X-Auth-Timestamp", timestamp },
                { "X-Auth-Version", "v2" }
                //{ "Content-Type", contentType }  //Results in an exception
            };
        }

        private string ByteToString(byte[] buff)
        {
            var sbinary = "";
            foreach (var t in buff)
                sbinary += t.ToString("X2"); /* hex format */
            return sbinary;
        }
    }
}