using CryptoControlCenter.Common.Enums;
using CryptoControlCenter.Common.Models;
using CryptoControlCenter.Common.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CryptoControlCenter.Common.Helper
{
    public static class Extensions
    {
        //TODO Stablecoins
        #region Stablecoins
        /// <summary>
        /// This is a list of common USD stablecoins. This array is not complete, but it shows the main stablecoins (by market capitalization)
        /// </summary>
        private static readonly string[] CommonUSDStableCoins = { "USDT", "USDC", "BUSD", "DAI", "UST", "TUSD", "USDP", "USDN", "HUSD", "FEI", "LUSD", "FRAX", "SUSD", "ALUSD", "GUSD", "VAI", "USDX", "CUSD" };
        /// <summary>
        /// This is a list of common EUR stablecoins. This array is not complete, but it shows the main stablecoins (by market capitalization)
        /// </summary>
        private static readonly string[] CommonEURStableCoins = { "SEUR", "CEUR", "EURT", "EURS", "PAR", "JEUR" };
        #endregion

        //TODO FIAT Currencies
        #region FIAT Currencies
        /// <summary>
        /// This is a list of FIAT currencies. This array is not complete, but it shows the currencies necessary for this program to work
        /// </summary>
        private static readonly string[] FIATcurrencies = { "EUR", "USD", "GBP" };
        #endregion

        /// <summary>
        /// Indicates if an asset is a USD stablecoin
        /// </summary>
        public static bool IsUSDStablecoin(this string asset)
        {
            if (string.IsNullOrWhiteSpace(asset))
            {
                return false;
            }
            else return CommonUSDStableCoins.Contains(asset.ToUpper());
        }
        /// <summary>
        /// Indicates if an asset is a EUR stablecoin
        /// </summary>
        public static bool IsEURstablecoin(this string asset)
        {
            if (string.IsNullOrWhiteSpace(asset))
            {
                return false;
            }
            else return CommonEURStableCoins.Contains(asset.ToUpper());
        }
        /// <summary>
        /// Indicates if an asset is a FIAT currency
        /// </summary>
        public static bool IsFIATcurrency(this string asset)
        {
            if (string.IsNullOrWhiteSpace(asset))
            {
                return false;
            }
            else return FIATcurrencies.Contains(asset.ToUpper());
        }


        /// <summary>
        /// Indicates, if an DateTime is within a specified TimeSpan compared to an other DateTime.
        /// If ts is positive, read it like "Will dt happen in the future ts-Amount relative to compareDT?"
        /// If ts is negative, read it like "Did dt happened the past ts-Amount relative to compareDT?"
        /// </summary>
        /// <param name="dt">DateTime</param>
        /// <param name="compareDt">DateTime</param>
        /// <param name="ts">TimeSpan Range</param>
        /// <returns>true, when dt is within cDT and cDT+ts, otherwise false</returns>
        public static bool IsWithinTimeSpan(this DateTime dt, DateTime compareDt, TimeSpan ts)
        {
            if (dt == compareDt)
            {
                return true;
            }
            else
            {
                var cDTts = compareDt + ts;
                TimeSpan ts0 = new TimeSpan(0);
                if (ts > ts0)
                {
                    if (dt >= compareDt && dt <= cDTts)
                    {
                        return true;
                    }
                    else return false;
                }
                else
                {
                    if (dt >= cDTts && dt <= compareDt)
                    {
                        return true;
                    }
                    else return false;
                }
            }
        }
        /// <summary>
        /// This method processes a transaction with regard to the update of the HodledAssets and the AnnualFinancialStatements (including §23 EStG)
        /// </summary>
        /// <param name="transaction">The transaction, which needs to process</param>
        /// <param name="fsDictionary">Reference to the Annual Financial Statement Helper</param>
        /// <param name="hodledAssets">Reference to the HodledAssets Set</param>
        /// <param name="lfdNr">current Position Number in Worksheet</param>
        /// <exception cref="InvalidOperationException">Thrown, when HodledAsset does not contain an to be sold/withdrawn asset</exception>
        /// <returns>ProcessResult, indicating if §23 occured</returns>
        public static ProcessResult Process(this ITransactionViewer transaction, ref Dictionary<string, FinancialStatementHelper> fsDictionary, ref SortedSet<HodledAsset> hodledAssets)
        {
            decimal left = transaction.AmountStart;
            decimal leftFee = transaction.FeeAmount;
            decimal buy = 0.0m;
            decimal buy23 = 0.0m;
            decimal fee = 0.0m;
            decimal fee23 = 0.0m;
            decimal sell = 0.0m;
            decimal sell23 = 0.0m;
            bool isLeapYear = false;

            switch (transaction.TransactionType)
            {
                case TransactionType.Buy:
                case TransactionType.Sell:
                    #region Asset
                    //Buy with EUR is excluded, as it is no sell of a taxable currency
                    while (left > 0.0m)
                    {
                        try
                        {
                            var asset = hodledAssets.First(x => x.removed == false && x.Asset == transaction.AssetStart && x.Location == transaction.Wallet);
                            if ((DateTime.IsLeapYear(asset.Received.Year) && asset.Received < new DateTime(asset.Received.Year, 2, 29)) || (DateTime.IsLeapYear(transaction.TransactionTime.Year) && transaction.TransactionTime > new DateTime(transaction.TransactionTime.Year, 3, 1)))
                            {
                                isLeapYear = true;
                            }
                            else
                            {
                                isLeapYear = false;
                            }
                            if (asset.CurrentAmount <= left)
                            {
                                if (transaction.AssetStart != "EUR") //excludes cases, where a crypto is bought with EUR as this is not added to afs
                                {
                                    if (asset.Received.IsWithinTimeSpan(transaction.TransactionTime, new TimeSpan(isLeapYear ? -365 : -364, -23, -59, -59)))
                                    {
                                        buy += asset.CurrentValueOnBuyRate;
                                        sell += asset.CurrentAmount / transaction.AmountStart * transaction.TransferValue;
                                    }
                                    else
                                    {
                                        fsDictionary.First(x => x.Key == transaction.Wallet).Value.paragraph23estg = true;
                                        buy23 += asset.CurrentValueOnBuyRate;
                                        sell23 += asset.CurrentAmount / transaction.AmountStart * transaction.TransferValue;
                                    }
                                }
                                left -= asset.CurrentAmount;
                                asset.CurrentAmount = 0.0m;
                                asset.CurrentValueOnBuyRate = 0.0m;
                                asset.removed = true;
                            }
                            else
                            {
                                if (transaction.AssetStart != "EUR") //excludes cases, where a crypto is bought with EUR as this is not added to afs
                                {
                                    if (asset.Received.IsWithinTimeSpan(transaction.TransactionTime, new TimeSpan(isLeapYear ? -365 : -364, -23, -59, -59)))
                                    {
                                        buy += left / asset.OriginalAmount * asset.OriginalValue;
                                        sell += left / transaction.AmountStart * transaction.TransferValue;
                                    }
                                    else
                                    {
                                        fsDictionary.First(x => x.Key == transaction.Wallet).Value.paragraph23estg = true;
                                        buy23 += left / asset.OriginalAmount * asset.OriginalValue;
                                        sell23 += left / transaction.AmountStart * transaction.TransferValue;
                                    }
                                }
                                asset.CurrentValueOnBuyRate -= left / asset.OriginalAmount * asset.OriginalValue;
                                asset.CurrentAmount -= left;
                                left = 0.0m;
                            }
                        }
                        catch (InvalidOperationException)
                        {
                            var leftValue = left / transaction.AmountStart * transaction.TransferValue;
                            if (leftValue < 1.00m) //Catches some errors due to rounding of APIs. When leftValue is less than 1 €, the exception gets ignored
                            {
                                left = 0.0m;
                            }
                            else
                            {
                                throw new InvalidOperationException("HodledAssets did not contain an asset, that needs to be sold.");
                            }
                        }
                    }
                    #endregion
                    //Insert new Asset into Set
                    hodledAssets.Add(new HodledAsset(transaction.LocationDestination, transaction.AssetDestination, transaction.AmountDestination, transaction.TransferValue, transaction.TransactionTime));
                    #region Fee
                    if (transaction.FeeAsset == transaction.AssetDestination)
                    {
                        var feeAsset = hodledAssets.Last(x => x.Asset == transaction.FeeAsset);
                        feeAsset.CurrentValueOnBuyRate -= leftFee / feeAsset.OriginalAmount * feeAsset.OriginalValue;
                        feeAsset.CurrentAmount -= leftFee;
                        leftFee = 0.0m;
                    }
                    else
                    {
                        while (leftFee > 0.0m)
                        {
                            try
                            {
                                if (!string.IsNullOrWhiteSpace(transaction.FeeAsset))
                                {
                                    var feeAsset = hodledAssets.First(x => x.Asset == transaction.FeeAsset && x.removed == false && x.Location == transaction.Wallet);
                                    if (feeAsset.CurrentAmount <= leftFee)
                                    {
                                        leftFee -= feeAsset.CurrentAmount;
                                        feeAsset.CurrentAmount = 0.0m;
                                        feeAsset.CurrentValueOnBuyRate = 0.0m;
                                        feeAsset.removed = true;
                                    }
                                    else
                                    {
                                        feeAsset.CurrentValueOnBuyRate -= leftFee / feeAsset.OriginalAmount * feeAsset.OriginalValue;
                                        feeAsset.CurrentAmount -= leftFee;
                                        leftFee = 0.0m;
                                    }
                                }
                            }
                            catch (InvalidOperationException)
                            {
                                var leftFeeValue = leftFee / transaction.FeeAmount * transaction.FeeValue;
                                if (leftFeeValue < 0.05m) //Catches some errors due to rounding of APIs. When leftFeeValue is less than 5ct, the exception gets ignored
                                {
                                    leftFee = 0.0m;
                                }
                                else
                                {
                                    throw new InvalidOperationException("Hodled Assets did not contain an asset, that is fee.");
                                }
                            }
                        }
                    }
                    if (transaction.AssetStart == "EUR") //when a crypto is bought with EUR the fee is nontheless added, but due line //note not calculatable like below
                    {
                        fee = transaction.FeeValue;
                    }
                    else
                    {
                        if (sell == 0.0m && sell23 != 0.0m)
                        {
                            fee23 = transaction.FeeValue;
                        }
                        else if (sell != 0.0m && sell23 == 0.0m)
                        {
                            fee = transaction.FeeValue;
                        }
                        else
                        {
                            fee = (sell / (sell + sell23)) * transaction.FeeValue;
                            fee23 = (sell23 / (sell + sell23)) * transaction.FeeValue;
                        }
                    }
                    #endregion
                    break;
                case TransactionType.Transfer:
                    if (transaction.LocationDestination == transaction.Wallet)
                    {
                        if (transaction.LocationStart == "Unbekanntes Wallet" || string.IsNullOrWhiteSpace(transaction.LocationStart)) //deposits normally gets dropped and done through withdrawal action, but when origin is unknown, create dummy asset
                        {
                            hodledAssets.Add(new HodledAsset(transaction.LocationDestination, transaction.AssetDestination, transaction.AmountDestination, transaction.TransferValue, transaction.TransactionTime));
                        }
                    }
                    else //Withdraw from this wallet 
                    {
                        #region Asset
                        while (left > 0.0m)
                        {
                            try
                            {
                                //Transfers are not taxed nor do they change the receive date and affect the one-year-holding duration
                                var asset = hodledAssets.First(x => x.Asset == transaction.AssetStart && x.removed == false && x.Location == transaction.Wallet);
                                if (asset.CurrentAmount <= left)
                                {
                                    left -= asset.CurrentAmount;
                                    asset.Location = transaction.LocationDestination;
                                }
                                else
                                {
                                    hodledAssets.Add(asset.Split(left, transaction.LocationDestination));
                                    left = 0.0m;
                                }
                            }
                            catch (InvalidOperationException)
                            {
                                var leftValue = left / transaction.AmountStart * transaction.TransferValue;
                                if (leftValue < 1.00m) //Catches some errors due to rounding of APIs. When leftValue is less than 1 €, the exception gets ignored
                                {
                                    left = 0.0m;
                                }
                                else
                                {
                                    throw new InvalidOperationException("Hodled Assets did not contain an asset, that needs to be withdrawn.");
                                }
                            }
                        }
                        #endregion
                        #region Fee
                        while (leftFee > 0.0m)
                        {
                            try
                            {
                                if (!string.IsNullOrWhiteSpace(transaction.FeeAsset))
                                {
                                    var feeAsset = hodledAssets.First(x => x.Asset == transaction.FeeAsset && x.Location == transaction.LocationStart && x.removed == false);
                                    if (feeAsset.CurrentAmount <= leftFee)
                                    {
                                        leftFee -= feeAsset.CurrentAmount;
                                        feeAsset.CurrentAmount = 0.0m;
                                        feeAsset.CurrentValueOnBuyRate = 0.0m;
                                        feeAsset.removed = true;
                                    }
                                    else
                                    {
                                        feeAsset.CurrentValueOnBuyRate -= leftFee / feeAsset.OriginalAmount * feeAsset.OriginalValue;
                                        feeAsset.CurrentAmount -= leftFee;
                                        leftFee = 0.0m;
                                    }
                                }
                            }
                            catch (InvalidOperationException)
                            {
                                var leftFeeValue = leftFee / transaction.FeeAmount * transaction.FeeValue;
                                if (leftFeeValue < 0.05m) //Catches some errors due to rounding of APIs. When leftFeeValue is less than 5ct, the exception gets ignored
                                {
                                    leftFee = 0.0m;
                                }
                                else
                                {
                                    throw new InvalidOperationException("Hodled Assets did not contain an asset, that is fee.");
                                }
                            }
                        }
                        //Fees gets added nonetheless transfer is not taxed, as they are still expenses
                        fee = transaction.FeeValue;
                        #endregion
                    }
                    break;
                case TransactionType.Distribution:
                    //Insert Distribution into HodledAsset-Set, but with value = 0 -> AFS-Buys will not increase, but Sells as soon as it is sold
                    //When distribution amount is negative, it is most commonly a swap
                    if (transaction.AmountStart > 0.0m)
                    {
                        hodledAssets.Add(new HodledAsset(transaction.LocationStart, transaction.AssetDestination, transaction.AmountDestination, 0.0m, transaction.TransactionTime));
                    }
                    else
                    {
                        left *= -1.0m;
                        while (left > 0.0m)
                        {
                            try
                            {
                                //Distribution swaps are not taxed nor do they change the receive date and affect the one-year-holding duration
                                var asset = hodledAssets.First(x => x.Asset == transaction.AssetStart && x.removed == false && x.Location == transaction.Wallet);
                                if (asset.CurrentAmount <= left)
                                {
                                    left -= asset.CurrentAmount;
                                    asset.removed = true;
                                }
                                else
                                {
                                    var a = asset.Split(left, transaction.LocationStart);
                                    a.removed = true;
                                    hodledAssets.Add(a);
                                    left = 0.0m;
                                }
                            }
                            catch (InvalidOperationException)
                            {
                                var leftValue = left / transaction.AmountStart * transaction.TransferValue;
                                if (leftValue < 1.00m) //Catches some errors due to rounding of APIs. When leftValue is less than 1 €, the exception gets ignored
                                {
                                    left = 0.0m;
                                }
                                else
                                {
                                    throw new InvalidOperationException("Hodled Assets did not contain an asset, that needs to be withdrawn.");
                                }
                            }
                        }
                    }
                    break;
                //TODO Dust
                case TransactionType.Dust:
                    throw new NotImplementedException();
                case TransactionType.BankDeposit:
                    hodledAssets.Add(new HodledAsset(transaction.LocationDestination, transaction.AssetDestination, transaction.AmountDestination, transaction.TransferValue, transaction.TransactionTime));
                    break;
                case TransactionType.BankWithdrawal:
                    #region Asset
                    while (left > 0.0m)
                    {
                        try
                        {
                            var asset = hodledAssets.First(x => x.Asset == transaction.AssetStart && x.removed == false && x.Location == transaction.LocationStart);
                            if ((DateTime.IsLeapYear(asset.Received.Year) && asset.Received < new DateTime(asset.Received.Year, 2, 29)) || (DateTime.IsLeapYear(transaction.TransactionTime.Year) && transaction.TransactionTime > new DateTime(transaction.TransactionTime.Year, 3, 1)))
                            {
                                isLeapYear = true;
                            }
                            else
                            {
                                isLeapYear = false;
                            }
                            if (asset.CurrentAmount <= left)
                            {
                                //BankWithdrawal is not added to Buys/Sells as EUR/USD is a FIAT-Currency and is therefore not taxed.
                                //Only update hodledAssets here
                                left -= asset.CurrentAmount;
                                asset.CurrentAmount = 0.0m;
                                asset.CurrentValueOnBuyRate = 0.0m;
                                asset.removed = true;
                            }
                            else
                            {
                                //BankWithdrawal is not added to Buys/Sells as EUR/USD is a FIAT-Currency and is therefore not taxed.
                                //Only update hodledAssets here
                                asset.CurrentValueOnBuyRate -= left / asset.OriginalAmount * asset.OriginalValue;
                                asset.CurrentAmount -= left;
                                left = 0.0m;
                            }
                        }
                        catch (InvalidOperationException)
                        {
                            throw new InvalidOperationException("Hodled Assets did not contain an FIAT asset, that needs to be withdrawn.");
                        }
                    }
                    #endregion
                    #region Fee
                    while (leftFee > 0.0m)
                    {
                        try
                        {
                            if (!string.IsNullOrWhiteSpace(transaction.FeeAsset))
                            {
                                var feeAsset = hodledAssets.First(x => x.Asset == transaction.FeeAsset && x.removed == false && x.Location == transaction.LocationStart);
                                if (feeAsset.CurrentAmount <= leftFee)
                                {
                                    leftFee -= feeAsset.CurrentAmount;
                                    feeAsset.CurrentAmount = 0.0m;
                                    feeAsset.CurrentValueOnBuyRate = 0.0m;
                                    feeAsset.removed = true;
                                }
                                else
                                {
                                    feeAsset.CurrentValueOnBuyRate -= leftFee / feeAsset.OriginalAmount * feeAsset.OriginalValue;
                                    feeAsset.CurrentAmount -= leftFee;
                                    leftFee = 0.0m;
                                }
                            }
                        }
                        catch (InvalidOperationException)
                        {
                            var leftFeeValue = leftFee / transaction.FeeAmount * transaction.FeeValue;
                            if (leftFeeValue < 0.05m) //Catches some errors due to rounding of APIs. When leftFeeValue is less than 5ct, the exception gets ignored
                            {
                                leftFee = 0.0m;
                            }
                            else
                            {
                                throw new InvalidOperationException("Hodled Assets did not contain an asset, that is fee.");
                            }
                        }
                    }
                    //Fees however are added to AFS, as they are expenses, nontheless EUR/USD is not taxed.
                    fee = transaction.FeeValue;
                    #endregion
                    break;
                default:
                    throw new NotImplementedException();
            }
            fsDictionary.First(x => x.Key == transaction.Wallet).Value.summedBuys += decimal.Round(buy, 2);
            fsDictionary.First(x => x.Key == transaction.Wallet).Value.summedBuys23 += decimal.Round(buy23, 2);
            fsDictionary.First(x => x.Key == transaction.Wallet).Value.summedSells += decimal.Round(sell, 2);
            fsDictionary.First(x => x.Key == transaction.Wallet).Value.summedSells23 += decimal.Round(sell23, 2);
            fsDictionary.First(x => x.Key == transaction.Wallet).Value.summedFees += decimal.Round(fee, 2);
            fsDictionary.First(x => x.Key == transaction.Wallet).Value.summedFees23 += decimal.Round(fee23, 2);
            if (buy23 != 0.0m && buy != 0.0m)
            {
                return ProcessResult.PartialParagraph23;
            }
            else if (buy23 != 0.0m)
            {
                return ProcessResult.FullParagraph23;
            }
            else return ProcessResult.NoParagraph23;
        }
        ///// <summary>
        ///// Unites similar assets within an HodledAsset-Dictionary (e.g. if an order was splitted into multiple transactions)
        ///// </summary>
        //public static Dictionary<int, SortedSet<HodledAsset>> UniteAssets(this Dictionary<int, SortedSet<HodledAsset>> dictionary)
        //{
        //    Dictionary<int, SortedSet<HodledAsset>> result = new Dictionary<int, SortedSet<HodledAsset>>();
        //    foreach (KeyValuePair<int, SortedSet<HodledAsset>> kvp in dictionary)
        //    {
        //        List<HodledAsset> unitedAssets = new List<HodledAsset>();
        //        foreach (HodledAsset asset in kvp.Value)
        //        {
        //            if (!unitedAssets.Any(x => x.Asset == asset.Asset)) //If asset does not exist yet -> add it
        //            {
        //                unitedAssets.Add(asset);
        //            }
        //            else
        //            {
        //                bool united = false;
        //                foreach (HodledAsset temp in unitedAssets.Where(x => x.Asset == asset.Asset))  //check if it can be united with an existing -> unite
        //                {
        //                    if (!united)
        //                    {
        //                        if (temp.Received.IsWithinTimeSpan(asset.Received, new TimeSpan(0, -5, 0)))
        //                        {
        //                            temp.Unite(asset);
        //                            united = true;
        //                        }
        //                    }
        //                }
        //                if (!united) //if not able to united -> add it
        //                {
        //                    unitedAssets.Add(asset);
        //                }
        //            }
        //        }
        //        SortedSet<HodledAsset> sorted = new SortedSet<HodledAsset>();
        //        foreach(HodledAsset asset in unitedAssets)
        //        {
        //            sorted.Add(asset);
        //        }
        //        result.Add(kvp.Key, sorted);
        //    }
        //    return result;
        //}
        ///// <summary>
        ///// Shows a simplified version of the HodledAssets-Dictionary, where all same assets regardless of the date of receipt and value are united
        ///// </summary>
        //public static Dictionary<int, SortedSet<HodledAsset>> GetBalances(this Dictionary<int, SortedSet<HodledAsset>> dictionary)
        //{
        //    Dictionary<int, SortedSet<HodledAsset>> result = new Dictionary<int, SortedSet<HodledAsset>>();
        //    foreach (KeyValuePair<int, SortedSet<HodledAsset>> kvp in dictionary)
        //    {
        //        List<HodledAsset> unitedAssets = new List<HodledAsset>();
        //        foreach (HodledAsset asset in kvp.Value)
        //        {
        //            if (!unitedAssets.Any(x => x.Asset == asset.Asset)) //If asset does not exist yet -> add it
        //            {
        //                unitedAssets.Add(asset);
        //            }
        //            else //else unite it with the existing entry
        //            {
        //                unitedAssets.First(x => x.Asset == asset.Asset).Unite(asset);
        //            }
        //        }
        //        SortedSet<HodledAsset> sorted = new SortedSet<HodledAsset>();
        //        foreach (HodledAsset asset in unitedAssets)
        //        {
        //            sorted.Add(asset);
        //        }
        //        result.Add(kvp.Key, sorted);
        //    }
        //    return result;
        //}
    }
}