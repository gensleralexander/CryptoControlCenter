using Bitstamp.Net.Enums;
using Bitstamp.Net.Objects;
using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace Bitstamp.Net.Interfaces.SubClients
{
    public interface IBitstampApiPrivateClient
    {
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
        Task<WebCallResult<IEnumerable<BitstampUserTransaction>>> GetUserTransactionsAsync(string tradingpair = null, int offset = 0, int limit = 1000, SortingOrder sort = SortingOrder.Descending, DateTime? since_timestamp = null, long? since_id = null, CancellationToken ct = default);

        /// <summary>
        /// Queries the Crypto Withdrawals and Deposits. Note: Most informations also get queried by GetUserTransactionAsync, so this method is only recommended, if you need the very specific crypto-transactions data.
        /// </summary>
        /// <param name="offset">Skip that many transactions before returning results (default: 0, maximum: 200000).</param>
        /// <param name="limit">Limit result to that many transactions (default: 100; maximum: 1000).</param>
        /// <param name="includeRippleIOUs">Shows also ripple IOU transactions</param>
        /// <param name="ct">Task Cancellation Token</param>
        /// <returns>IEnumerable of UserTransactions</returns>
        Task<WebCallResult<BitstampCryptoTransactions>> GetCryptoDepositsAndWithdrawalsAsync(int offset = 0, int limit = 1000, bool includeRippleIOUs = true, CancellationToken ct = default);
    }
}
