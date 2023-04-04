using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CryptoControlCenter.Common.Models
{
    /// <summary>
    /// This is a helper-class only used to save request details and results for exchange rates
    /// </summary>
    public class ExchangeRateRequest : IComparable
    {
        public DateTime DateTime { get; set; }
        public string Asset { get; set; }
        public string BaseAsset { get; set; }
        public IEnumerable<ExchangeRate> Rates { get; set; }

        public ExchangeRateRequest(DateTime dateTime, string asset)
        {
            DateTime = dateTime;
            Asset = asset;
        }
        public ExchangeRateRequest(DateTime dateTime, string asset, string baseAsset)
        {
            DateTime = dateTime;
            Asset = asset;
            BaseAsset = baseAsset;
        }

        public int CompareTo(object item)
        {
            if (DateTime != ((ExchangeRateRequest)item).DateTime)
            {
                return DateTime.CompareTo(((ExchangeRateRequest)item).DateTime);  //if TransactionTimes are not equal, compare and sort the request by this
            }
            else if (JsonConvert.SerializeObject(this) == JsonConvert.SerializeObject(item))
            {
                return 0;   //if Transaction times are equal, compare if both transactions have same values (is a duplicate)
            }
            else return 1;  //else return 1 -> item is "greater" means it gets sorted after this.
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
