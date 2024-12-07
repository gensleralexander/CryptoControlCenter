using CryptoControlCenter.Common.Models.Interfaces;
using Syncfusion.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace CryptoControlCenter.WPF.Helper
{
    public class AmountAggregate : ISummaryAggregate
    {
        public AmountAggregate() { }

        public decimal CurrentAmount { get; set; }

        public Action<IEnumerable, string, PropertyDescriptor> CalculateAggregateFunc()
        {
            return (items, property, pd) =>
            {
                var enumerableItems = items as IEnumerable<IBalanceViewer>;

                if (pd.Name == "CurrentAmount" && enumerableItems != null)
                {
                    CurrentAmount = enumerableItems.AmountAggregation<IBalanceViewer>();
                }
            };
        }
    }
}
