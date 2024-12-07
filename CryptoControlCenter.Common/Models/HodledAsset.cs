using Newtonsoft.Json;
using System;
using System.Text;
using System.Runtime.CompilerServices;
using CryptoControlCenter.Common.Helper;
using CryptoControlCenter.Common.Models.Interfaces;

[assembly: InternalsVisibleTo("CryptoControlCenter.Common.UnitTests")]
namespace CryptoControlCenter.Common.Models
{
    /// <summary>
    /// This class describes an asset, that is hodled.
    /// </summary>
    public class HodledAsset : AbstractPropertyChanged, IComparable, IBalanceViewer
    {
        private string wallet;
        private string asset;
        private decimal currentAmount;
        private decimal originalAmount;
        private decimal currentValue;
        private decimal originalValue;
        private decimal currentValueOnBuyRate;
        private DateTime received;

        public Guid Guid { get; }
        /// <inheritdoc />
        public string Wallet
        {
            get { return wallet; }
            set
            {
                wallet = value;
                OnPropertyChanged();
            }
        }
        /// <inheritdoc />
        public string Asset
        {
            get { return asset; }
            set
            {
                asset = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Hodled Amount
        /// </summary>
        public decimal OriginalAmount
        {
            get { return originalAmount; }
            set
            {
                originalAmount = value;
                OnPropertyChanged();
            }
        }
        /// <inheritdoc />
        public decimal CurrentAmount
        {
            get
            {
                return currentAmount;
            }
            internal set
            {
                currentAmount = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Value of Hodled Amount on original amount
        /// </summary>
        public decimal OriginalValue
        {
            get
            { return originalValue; }
            set
            {
                originalValue = value;
                OnPropertyChanged();
            }
        }
        /// <inheritdoc />
        public decimal CurrentValue
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
        /// <inheritdoc />
        public decimal CurrentValueOnBuyRate
        {
            get
            {
                return currentValueOnBuyRate;
            }
            internal set
            {
                currentValueOnBuyRate = value;
                OnPropertyChanged();
            }
        }
        /// <inheritdoc />
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
        /// <inheritdoc />
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
        /// <inheritdoc />
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
        /// <inheritdoc />
        public decimal Profit
        {
            get
            {
                if (currentValueOnBuyRate == 0.0m)
                {
                    return 0.0m;
                }
                else return ((CurrentValue / CurrentValueOnBuyRate) - 1) * 100;
            }
        }
        /// <summary>
        /// Bug in SortedSet.RemoveWhere → mark as removed and dont use again
        /// </summary>
        internal bool removed { get; set; }


        /// <summary>
        /// Constructor
        /// </summary>
        public HodledAsset(string location, string asset, decimal amount, decimal value, DateTime received)
        {
            if (amount == 0)
            {
                throw new ArgumentException("Amount cannot be zero");
            }
            Guid = Guid.NewGuid();
            Wallet = location;
            Asset = asset;
            OriginalAmount = amount;
            CurrentAmount = amount;
            Received = received;
            OriginalValue = value;
            CurrentValueOnBuyRate = value;
        }
        /// <summary>
        /// Unites two similar assets (e.g. if an order was splitted into multiple transactions). The possibility check for uniting has to be done before.
        /// </summary>
        /// <param name="otherAsset">To be united asset</param>
        public void Unite(HodledAsset otherAsset)
        {
            OriginalAmount += otherAsset.OriginalAmount;
            CurrentAmount += otherAsset.CurrentAmount;
            OriginalValue += otherAsset.OriginalValue;
            CurrentValueOnBuyRate += otherAsset.CurrentValueOnBuyRate;
        }

        /// <summary>
        /// Splits current asset and moves the new asset to a new location. New Asset has same received time (as split is used by Type Transfer, which is not considered a sell/buy)
        /// </summary>
        /// <param name="amount">Amount to be split into new asset.</param>
        /// <param name="newLocation">Walletname-Location of new asset</param>
        /// <exception cref="InvalidOperationException">Occurs, when parameter amount is higher or equal to current assets amount.</exception>
        /// <returns></returns>
        public HodledAsset Split(decimal amount, string newLocation)
        {
            if (amount >= CurrentAmount)
            {
                throw new InvalidOperationException("Parameter amount was higher or equal of current assets amount: " + CurrentAmount.ToString());
            }
            else
            {
                CurrentValueOnBuyRate -= (amount * OriginalValue / OriginalAmount);
                CurrentAmount -= amount;
                return new HodledAsset(newLocation, Asset, amount, (amount * OriginalValue / OriginalAmount), Received);
            }
        }

        /// <summary>
        /// IComparable Implementation
        /// </summary>
        public int CompareTo(object item)
        {
            HodledAsset hodled = (HodledAsset)item;
            if (Received != hodled.Received)
            {
                return Received.CompareTo(hodled.Received);  //if ReceiveTimes are not equal, compare and sort the assets by this
            }
            else if (Wallet != hodled.Wallet)
            {
                return Wallet.CompareTo(hodled.Wallet); //if Locations are not equal, compare and sort alphabetically by walletname
            }
            else if (Asset != hodled.Asset)
            {
                return Asset.CompareTo(hodled.Asset); // compare by Asset
            }
            if (JsonConvert.SerializeObject(this) == JsonConvert.SerializeObject(hodled))
            {
                return 0;   //check if both transactions have same values (is a duplicate)
            }
            else return 1;  // else return 1 -> item is "greater" means it gets sorted after this.
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Asset: ");
            sb.AppendLine(Asset);
            sb.Append("Wallet: ");
            sb.AppendLine(Wallet);
            sb.Append("Original Amount: ");
            sb.AppendLine(OriginalAmount.ToString());
            sb.Append("Left Amount: ");
            sb.AppendLine(CurrentAmount.ToString());
            sb.Append("Original Value: ");
            sb.AppendLine(OriginalValue.ToString());
            sb.Append("Left Value: ");
            sb.AppendLine(CurrentValueOnBuyRate.ToString());
            sb.Append("Received: ");
            sb.AppendLine(Received.ToString());
            return sb.ToString();
        }
    }
}