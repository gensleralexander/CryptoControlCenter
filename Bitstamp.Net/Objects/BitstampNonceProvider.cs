using System;

namespace Bitstamp.Net.Objects
{
    internal class BitstampNonceProvider
    {
        private static readonly object nonceLock = new object();
        private static Guid? lastNonce;

        /// <inheritdoc />
        public Guid GetNonce()
        {
            lock (nonceLock)
            {
                var nonce = Guid.NewGuid();
                while (lastNonce.HasValue && nonce == lastNonce)
                {
                    nonce = Guid.NewGuid();
                }
                lastNonce = nonce;
                return nonce;
            }
        }
    }
}
