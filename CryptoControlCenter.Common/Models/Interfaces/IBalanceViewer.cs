using System;

namespace CryptoControlCenter.Common.Models.Interfaces
{
    public interface IBalanceViewer
    {
        /// <summary>
        /// Parent Wallet
        /// </summary>
        public string Wallet { get; }
        /// <summary>
        /// Asset Symbol
        /// </summary>
        public string Asset { get; }
        /// <summary>
        /// Amount of Asset
        /// </summary>
        public double CurrentAmount { get; }
        /// <summary>
        /// Value of Asset
        /// </summary>
        public double CurrentValue { get; }
    }
}
