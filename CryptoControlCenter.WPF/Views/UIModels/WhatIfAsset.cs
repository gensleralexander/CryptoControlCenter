using CryptoControlCenter.Common.Helper;
using CryptoControlCenter.Common.Models.Interfaces;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace CryptoControlCenter.WPF.Views.UIModels
{
    public class WhatIfAsset : AbstractPropertyChanged, IEditableObject
    {
        private decimal whatIfPrice;
        public string Asset { get; }
        public decimal CurrentAmount { get; internal set; }
        public decimal CurrentValue { get; internal set; }
        public decimal CurrentValueOnBuyRate { get; internal set; }
        public decimal CurrentPrice { get; }
        public decimal WhatIfPrice
        {
            get
            {
                return whatIfPrice;
            }
            set
            {
                whatIfPrice = value;
                OnPropertyChanged();
                OnPropertyChanged("WhatIfValue");
                OnPropertyChanged("WhatIfProfitCurrent");
                OnPropertyChanged("WhatIfProfitBuy");
            }
        }

        public decimal WhatIfValue
        {
            get
            {
                return whatIfPrice * CurrentAmount;
            }
        }

        public decimal WhatIfProfitCurrent
        {
            get
            {
                if (CurrentValue == 0.0m)
                {
                    return 0.0m;
                }
                else return ((WhatIfValue / CurrentValue) - 1) * 100;
            }
        }
        public decimal WhatIfProfitBuy
        {
            get
            {
                if (CurrentValueOnBuyRate == 0.0m)
                {
                    return 0.0m;
                }
                else return ((WhatIfValue / CurrentValueOnBuyRate) - 1) * 100;
            }
        }

        public WhatIfAsset(IBalanceViewer asset)
        {
            Asset = asset.Asset;
            CurrentAmount = asset.CurrentAmount;
            CurrentValue = asset.CurrentValue;
            CurrentValueOnBuyRate = asset.CurrentValueOnBuyRate;
            CurrentPrice = (asset.CurrentValue / asset.CurrentAmount);
            WhatIfPrice = (asset.CurrentValue / asset.CurrentAmount);
        }

        #region IEditable
        private Dictionary<string, object> storedValues;
        protected Dictionary<string, object> BackUp()
        {
            var dict = new Dictionary<string, object>();
            var itemProperties = this.GetType().GetTypeInfo().DeclaredProperties;

            foreach (var pDescriptor in itemProperties)
            {
                if (pDescriptor.CanWrite)
                    dict.Add(pDescriptor.Name, pDescriptor.GetValue(this));
            }
            return dict;
        }

        public void BeginEdit()
        {
            this.storedValues = this.BackUp();
        }

        public void CancelEdit()
        {
            if (this.storedValues == null)
                return;

            foreach (var item in this.storedValues)
            {
                var itemProperties = this.GetType().GetTypeInfo().DeclaredProperties;
                var pDesc = itemProperties.FirstOrDefault(p => p.Name == item.Key);

                if (pDesc != null)
                    pDesc.SetValue(this, item.Value);
            }
        }

        public async void EndEdit()
        {
            if (this.storedValues != null)
            {
                OnPropertyChanged("WhatIfPrice");
                OnPropertyChanged("WhatIfValue");
                OnPropertyChanged("WhatIfProfitBuy");
                OnPropertyChanged("WhatIfProfitCurrent");
                this.storedValues.Clear();
                this.storedValues = null;
            }
        }
        #endregion
    }
}
