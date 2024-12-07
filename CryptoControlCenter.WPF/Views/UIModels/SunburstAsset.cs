using CryptoControlCenter.Common.Models.Interfaces;
using System;
using CryptoControlCenter.Common.Helper;

namespace CryptoControlCenter.WPF.Views.UIModels
{
    /// <summary>
    /// This class is a converter model, as Syncfusions Sunburst only seems to be able to render, when the value is a Double-Type instead of Decimal
    /// </summary>
    public class SunburstAsset : AbstractPropertyChanged
    {
        private string asset;
        private double currentValue;
        private DateTime received;

        public string Asset
        {
            get { return asset; }
            set
            {
                asset = value;
                OnPropertyChanged();
            }
        }
        public double CurrentValue
        {
            get
            {
                return currentValue;
            }
            internal set
            {
                currentValue = value;
                OnPropertyChanged();
            }
        }
        public DateTime Received
        {
            get
            {
                return received;
            }
            internal set
            {
                received = value;
                OnPropertyChanged();
            }
        }
        public bool IsTaxfree
        {
            get
            {
                bool isLeapYear;
                if ((DateTime.IsLeapYear(Received.Year) && Received < new DateTime(Received.Year, 2, 29)) || (DateTime.IsLeapYear(DateTime.UtcNow.Year) && DateTime.UtcNow > new DateTime(DateTime.UtcNow.Year, 3, 1)))
                {
                    isLeapYear = true;
                }
                else
                {
                    isLeapYear = false;
                }
                //Is the asset younger than one year? > not taxfree
                return !Received.IsWithinTimeSpan(DateTime.UtcNow, new TimeSpan(isLeapYear ? -366 : -365, 0, 0, 0));
            }
        }
        public string TaxfreeString
        {
            get
            {
                if (IsTaxfree)
                {
                    return Resources.Strings.Yes;
                }
                else return Resources.Strings.No;
            }
        }

        public SunburstAsset(IBalanceViewer _asset)
        {
            Asset = _asset.Asset;
            Received = _asset.Received;
            CurrentValue = decimal.ToDouble(_asset.CurrentValue);
        }
    }
}
