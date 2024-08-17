using System;
using System.Collections.Generic;

namespace CryptoControlCenter.Common.Models
{
    /// <summary>
    /// This is a helper-class only used to save request details and results for exchange rates
    /// </summary>
    public class ExchangeRateRequest
    {
        public string Asset { get; set; }
        public List<ExchangeRate> Rates { get; set; }

        public ExchangeRateRequest(string asset)
        {
            Asset = asset;
            Rates = new List<ExchangeRate>();
        }
    }

    public class ExchangeRate
    {
        public DateTime OpenTime { get; set; }
        public DateTime CloseTime { get; set; }
        public decimal MedianRate { get; set; }

        public ExchangeRate() { }
        public ExchangeRate(DateTime open, DateTime close, decimal medianRate)
        {
            OpenTime = open;
            CloseTime = close;
            MedianRate = medianRate;
        }
    }
}
