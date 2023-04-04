using System;

namespace Bitstamp.Net.Interfaces
{
    public interface IBitstampKline
    {
        /// <summary>
        /// The highest price in this candlestick
        /// </summary>
        public decimal High { get; set; }
        /// <summary>
        /// The time this candlestick opened
        /// </summary>
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// The volume traded during this candlestick
        /// </summary>
        public decimal Volume { get; set; }
        /// <summary>
        /// The lowest price in this candlestick
        /// </summary>
        public decimal Low { get; set; }
        /// <summary>
        /// The price at which this candlestick closed
        /// </summary>
        public decimal Close { get; set; }
        /// <summary>
        /// The price at which this candlestick opened
        /// </summary>
        public decimal Open { get; set; }
    }
}