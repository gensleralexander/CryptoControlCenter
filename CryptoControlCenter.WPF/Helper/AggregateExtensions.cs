using CryptoControlCenter.Common.Models.Interfaces;
using CryptoControlCenter.WPF.Views.UIModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CryptoControlCenter.WPF.Helper
{
    public static class AggregateExtensions
    {
        /// <summary>
        /// Used for AmountAggregate
        /// </summary>
        public static decimal AmountAggregation<T>(this IEnumerable<T> enumerable)
        {
            decimal amount = 0.0m;
            var values = enumerable as IEnumerable<IBalanceViewer>;
            if (values != null && values.Count() > 0)
            {
                var val = values.First().Asset;
                bool notMixed = values.Skip(1).All(x => x.Asset == val);
                if (notMixed)
                {
                    foreach (var item in values)
                    {
                        amount += item.CurrentAmount;
                    }
                }
            }
            return amount;
        }
        /// <summary>
        /// Used for ProfitAggregate
        /// </summary>
        public static decimal ProfitAggregation<T>(this IEnumerable<T> enumerable, string selector)
        {
            decimal whatifvalue = 0.0m;
            decimal currentvalue = 0.0m;
            decimal currentvaluebuyrate = 0.0m;
            var values = enumerable as IEnumerable<WhatIfAsset>;
            if (values != null && values.Count() > 0)
            {
                foreach (var item in values)
                {
                    whatifvalue += item.WhatIfValue;
                    currentvalue += item.CurrentValue;
                    currentvaluebuyrate += item.CurrentValueOnBuyRate;
                }
                switch (selector)
                {
                    case "ProfitBuy":
                        if (currentvaluebuyrate != 0.0m)
                        {
                            return (whatifvalue / currentvaluebuyrate) - 1;
                        }
                        else return 0.0m;
                    case "ProfitCurrent":
                        if (currentvalue != 0.0m)
                        {
                            return (whatifvalue / currentvalue) - 1;
                        }
                        else return 0.0m;
                    default:
                        break;
                }
            }
            return 0.0m;
        }
    }
}
