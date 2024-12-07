using CryptoControlCenter.WPF.Views.UIModels;
using Syncfusion.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace CryptoControlCenter.WPF.Helper
{
    public class ProfitAggregate : ISummaryAggregate
    {
        public ProfitAggregate() { }

        public decimal ProfitBuy { get; set; }
        public decimal ProfitCurrent { get; set; }
        public Action<IEnumerable, string, PropertyDescriptor> CalculateAggregateFunc()
        {
            return (items, property, pd) =>
            {
                var enumerableItems = items as IEnumerable<WhatIfAsset>;

                if (pd.Name == "ProfitCurrent"&& enumerableItems != null)
                {
                    ProfitCurrent = enumerableItems.ProfitAggregation<WhatIfAsset>(pd.Name);
                }
                else if (pd.Name == "ProfitBuy" && enumerableItems != null)
                {
                    ProfitBuy = enumerableItems.ProfitAggregation<WhatIfAsset>(pd.Name);
                }
            };
        }
    }
}
