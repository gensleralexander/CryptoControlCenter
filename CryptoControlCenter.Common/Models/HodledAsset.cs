using Newtonsoft.Json;
using System;
using System.Text;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("CryptoControlCenter.Common.UnitTests")]
namespace CryptoControlCenter.Common.Models
{
    /// <summary>
    /// This class describes an asset, that is hodled.
    /// </summary>
    public class HodledAsset : IComparable
    {
        public Guid Guid { get; }
        /// <summary>
        /// PK-String Walletname
        /// </summary>
        public string Location { get; set; }
        /// <summary>
        /// Asset Symbol
        /// </summary>
        public string Asset { get; }
        /// <summary>
        /// Hodled Amount
        /// </summary>
        public decimal OriginalAmount { get; set; }
        /// <summary>
        /// Hodled Amount
        /// </summary>
        public decimal CurrentAmount { get; set; }
        /// <summary>
        /// Value of Hodled Amount
        /// </summary>
        public decimal OriginalValue { get; set; }
        /// <summary>
        /// Value of Hodled Amount
        /// </summary>
        public decimal CurrentValueOnBuyRate { get; set; }
        /// <summary>
        /// DateTime, when Asset was received/bought
        /// </summary>
        public DateTime Received { get; }
        /// <summary>
        /// Bug in SortedSet.RemoveWhere → mark as removed and dont use again
        /// </summary>
        internal bool removed { get; set; }
        /// <summary>
        /// Constructor
        /// </summary>
        public HodledAsset(string location, string asset, decimal amount, decimal value, DateTime received)
        {
            if(amount == 0)
            {
                throw new ArgumentException("Amount cannot be zero");
            }
            Guid = Guid.NewGuid();
            Location = location;
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
            else if (Location != hodled.Location)
            {
                return Location.CompareTo(hodled.Location); //if Locations are not equal, compare and sort alphabetically by walletname
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
            sb.Append("Location: ");
            sb.AppendLine(Location);
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