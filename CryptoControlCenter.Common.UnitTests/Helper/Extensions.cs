using CryptoControlCenter.Common.Enums;
using CryptoControlCenter.Common.Helper;
using CryptoControlCenter.Common.Models;
using CryptoControlCenter.Common.Models.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CryptoControlCenter.Common.UnitTests.Helper
{
    /// <summary>
    /// Unit-Tests for CryptoControlCenter.Common.Helper
    /// </summary>
    [TestClass]
    public class Extensions
    {
        /// <summary>
        /// Indicates, if an DateTime is within a specified TimeSpan compared to an other DateTime
        /// If ts is positive, read it like "Will dt happen in the future ts-Amount relative to compareDT?"
        /// If ts is negative, read it like "Did dt happened the past ts-Amount relative to compareDT?"
        /// </summary>
        /// <param name="dt">DateTime (newer than cDT)</param>
        /// <param name="compareDt">DateTime (older than dt)</param>
        /// <param name="ts">TimeSpan Range</param>
        /// <returns>True, when dt is within cDT and cDT + ts</returns>
        [TestMethod]
        public void IsWithinTimeSpan()
        {
            TimeSpan ts1 = new TimeSpan(1, 0, 0, 0);
            TimeSpan ts2 = new TimeSpan(-1, 0, 0, 0);
            DateTime dt = new DateTime(2000, 1, 1, 0, 0, 0);

            DateTime cDT1 = new DateTime(1999, 12, 31, 0, 0, 1); //-23h59m59s
            DateTime cDT2 = new DateTime(1999, 12, 31, 0, 0, 0); //-24h
            DateTime cDT3 = new DateTime(1999, 12, 30, 23, 59, 59); //-24h1s
            DateTime cDT4 = new DateTime(2000, 1, 1, 23, 59, 59); //+23h59m59s
            DateTime cDT5 = new DateTime(2000, 1, 2, 0, 0, 0); //+24h
            DateTime cDT6 = new DateTime(2000, 1, 2, 0, 0, 1); //+24h1s
            //ts1 +1d
            bool result11 = dt.IsWithinTimeSpan(cDT1, ts1);//-23h59m59s true
            bool result12 = dt.IsWithinTimeSpan(cDT2, ts1);//-24h true
            bool result13 = dt.IsWithinTimeSpan(cDT3, ts1);//-24h1s false
            bool result14 = dt.IsWithinTimeSpan(dt, ts1);//0s true
            bool result15 = dt.IsWithinTimeSpan(cDT4, ts1);//+23h59m59s false
            bool result16 = dt.IsWithinTimeSpan(cDT5, ts1);//+24h false
            bool result17 = dt.IsWithinTimeSpan(cDT6, ts1);//+24h1s false
            //ts2 -1d
            bool result21 = dt.IsWithinTimeSpan(cDT1, ts2);//-23h59m59s false
            bool result22 = dt.IsWithinTimeSpan(cDT2, ts2);//-24h false
            bool result23 = dt.IsWithinTimeSpan(cDT3, ts2);//-24h1s false
            bool result24 = dt.IsWithinTimeSpan(dt, ts2);//0s true
            bool result25 = dt.IsWithinTimeSpan(cDT4, ts2);//+23h59m59s true
            bool result26 = dt.IsWithinTimeSpan(cDT5, ts2);//+24h true
            bool result27 = dt.IsWithinTimeSpan(cDT6, ts2);//+24h1s false

            Assert.AreEqual(true, result11, "IsWithinTimeSpan: result11 was not equal to expected");
            Assert.AreEqual(true, result12, "IsWithinTimeSpan: result12 was not equal to expected");
            Assert.AreEqual(false, result13, "IsWithinTimeSpan: result13 was not equal to expected");
            Assert.AreEqual(true, result14, "IsWithinTimeSpan: result14 was not equal to expected");
            Assert.AreEqual(false, result15, "IsWithinTimeSpan: result15 was not equal to expected");
            Assert.AreEqual(false, result16, "IsWithinTimeSpan: result16 was not equal to expected");
            Assert.AreEqual(false, result17, "IsWithinTimeSpan: result17 was not equal to expected");

            Assert.AreEqual(false, result21, "IsWithinTimeSpan: result21 was not equal to expected");
            Assert.AreEqual(false, result22, "IsWithinTimeSpan: result22 was not equal to expected");
            Assert.AreEqual(false, result23, "IsWithinTimeSpan: result23 was not equal to expected");
            Assert.AreEqual(true, result24, "IsWithinTimeSpan: result24 was not equal to expected");
            Assert.AreEqual(true, result25, "IsWithinTimeSpan: result25 was not equal to expected");
            Assert.AreEqual(true, result26, "IsWithinTimeSpan: result26 was not equal to expected");
            Assert.AreEqual(false, result27, "IsWithinTimeSpan: result27 was not equal to expected");
        }

        /// <summary>
        /// This method processes a transaction with regard to the update of the HodledAssets and the AnnualFinancialStatements (including §23 EStG)
        /// </summary>
        /// <param name="transaction">The transaction, which needs to process</param>
        /// <param name="afsHelper">Reference to the Annual Financial Statement Helper</param>
        /// <param name="hodledAssets">Reference to the HodledAssets Set</param>
        [TestMethod]
        public void Process()
        {
            Dictionary<string, FinancialStatementHelper> fsHelper = new Dictionary<string, FinancialStatementHelper>()
                {
                    { "StartDummy", new FinancialStatementHelper(0,0) },
                    { "Test", new FinancialStatementHelper(0,0) },
                    { "Test2", new FinancialStatementHelper(0,0) }
                };
            SortedSet<HodledAsset> set = new SortedSet<HodledAsset>()
            {
                new HodledAsset("StartDummy", "BTC", 100.0m, 100.0m, new DateTime(2000,1,1,0,0,0))
            };
            ProcessResult[] results = new ProcessResult[12];

            ITransactionViewer[] transactions = new Transaction[] {
                new Transaction("StartDummy", new DateTime(2000,1,1,0,0,1), TransactionType.Transfer, "BTC", "BTC", 100.0m, 100.0m, "StartDummy", "Test", 100.0m, 0.0m, "EUR", 0.0m), //t0
                new Transaction("Test", new DateTime(2000,12,31,23,0,0), TransactionType.Sell, "BTC", "USDT", 25.0m, 25.0m,"Test", "Test", 25.0m, 1.0m, "BTC", 1.0m), //t1
                new Transaction("Test", new DateTime(2000,12,31,23,30,0), TransactionType.Buy, "USDT", "BTC", 10.0m, 10.0m,"Test", "Test", 10.0m, 0.5m, "BTC", 0.5m), //t2
                new Transaction("Test", new DateTime(2001,1,1,0,0,0), TransactionType.Sell, "BTC", "EUR", 10.0m, 10.0m,"Test", "Test", 10.0m, 0.5m, "BTC", 0.5m), //t3
                new Transaction("Test", new DateTime(2001,12,31,23,15,0), TransactionType.Sell, "BTC", "ETH", 70.0m, 70.0m,"Test", "Test", 70.0m, 2.0m, "USDT", 2.0m), //t4
                new Transaction("Test", new DateTime(2001,12,31,23,20,0), TransactionType.Buy, "EUR", "BTC", 10.0m, 10.0m,"Test", "Test", 10.0m, 0.5m, "BTC", 0.5m), //t5
                new Transaction("Test", new DateTime(2002, 1,1,0,0,0), TransactionType.Transfer, "BTC", "BTC", 7.0m, 7.0m, "Test", "Test2", 7.0m, 0.5m, "BTC", 0.5m), //t6
                new Transaction("Test2", new DateTime(2002, 1,1,0,0,0), TransactionType.Transfer, "BTC", "BTC", 7.0m, 7.0m, "Test", "Test2", 7.0m, 0.0m, "EUR", 0.0m), //=t6 Test2-Side
                new Transaction("Test2", new DateTime(2002, 1,2,0,0,0), TransactionType.Transfer, "BTC", "BTC", 6.5m, 6.5m, "Test2", "Test", 65.0m, 0.5m, "BTC", 0.5m), //t7 Test2-Side
                new Transaction("Test", new DateTime(2002, 1,2,0,0,0), TransactionType.Transfer, "BTC", "BTC", 6.5m, 6.5m, "Test2", "Test", 65.0m, 0.0m, "EUR", 0.0m), //t7
                new Transaction("Test", new DateTime(2002, 1,2,0,0,0), TransactionType.Distribution, "TRX", "TRX", 10.0m, 10.0m,"Test", "Test", 0.0m, 0.0m, "EUR", 0.0m), //t8
                new Transaction("Test", new DateTime(2002, 1, 3, 0,0,0), TransactionType.Sell, "TRX", "USDT", 10.0m, 50.0m,"Test", "Test", 50.0m, 5.0m, "USDT", 5.0m ) //t9
            };

            #region t0 TransactionType.Transfer, "BTC", "BTC", 100.0m, 100.0m, "StartDummy", "Test", 100.0m, 0.0m, string.Empty
            results[0] = transactions[0].Process(ref fsHelper, ref set);
            Assert.AreEqual(ProcessResult.NoParagraph23, results[0], "Process: Result[0] not as expected");
            HodledAsset asset0 = new HodledAsset("Test", "BTC", 100.0m, 100.0m, new DateTime(2000, 1, 1, 0, 0, 0));
            Assert.IsTrue(TestingExtension.ObjectsEquals(asset0, set.ElementAt(0), "Guid"), "Process: HodledAsset0 was not equal to expected");
            #endregion

            #region t1 TransactionType.Sell, "BTC", "USDT", 25.0m, 25.0m,"Test", "Test", 25.0m, 1.0m, "BTC"){ FeeValue = 1.0m}
            results[1] = transactions[1].Process(ref fsHelper, ref set);
            Assert.AreEqual(ProcessResult.NoParagraph23, results[1], "Process: Result[1] not as expected");
            asset0.CurrentAmount = 74.0m;
            asset0.CurrentValueOnBuyRate = 74.0m;
            HodledAsset asset1 = new HodledAsset("Test", "USDT", 25.0m, 25.0m, new DateTime(2000, 12, 31, 23, 0, 0));
            Assert.IsTrue(TestingExtension.ObjectsEquals(asset0, set.ElementAt(0), "Guid"), "Process: HodledAsset0 was not equal to expected");
            Assert.IsTrue(TestingExtension.ObjectsEquals(asset1, set.ElementAt(1), "Guid"), "Process: HodledAsset1 was not equal to expected");
            Assert.AreEqual(25.0m, fsHelper.First(x => x.Key == "Test").Value.summedSells, "Process: SummedSells not as expected");
            Assert.AreEqual(25.0m, fsHelper.First(x => x.Key == "Test").Value.summedBuys, "Process: SummedBuys not as expected");
            Assert.AreEqual(1.0m, fsHelper.First(x => x.Key == "Test").Value.summedFees, "Process: SummedFees not as expected");
            #endregion

            #region t2 TransactionType.Buy, "USDT", "BTC", 10.0m, 10.0m,"Test", "Test", 10.0m, 0.5m, "BTC"){ FeeValue = 0.5m}
            results[2] = transactions[2].Process(ref fsHelper, ref set);
            Assert.AreEqual(ProcessResult.NoParagraph23, results[2], "Process: Result[2] not as expected");
            asset1.CurrentAmount = 15.0m;
            asset1.CurrentValueOnBuyRate = 15.0m;
            HodledAsset asset2 = new HodledAsset("Test", "BTC", 10.0m, 10.0m, new DateTime(2000, 12, 31, 23, 30, 0))
            {
                CurrentAmount = 9.5m,
                CurrentValueOnBuyRate = 9.5m
            };
            Assert.IsTrue(TestingExtension.ObjectsEquals(asset0, set.ElementAt(0), "Guid"), "Process: HodledAsset0 was not equal to expected");
            Assert.IsTrue(TestingExtension.ObjectsEquals(asset1, set.ElementAt(1), "Guid"), "Process: HodledAsset1 was not equal to expected");
            Assert.IsTrue(TestingExtension.ObjectsEquals(asset2, set.ElementAt(2), "Guid"), "Process: HodledAsset2 was not equal to expected");
            Assert.AreEqual(35.0m, fsHelper.First(x => x.Key == "Test").Value.summedSells, "Process: SummedSells not as expected");
            Assert.AreEqual(35.0m, fsHelper.First(x => x.Key == "Test").Value.summedBuys, "Process: SummedBuys not as expected");
            Assert.AreEqual(1.5m, fsHelper.First(x => x.Key == "Test").Value.summedFees, "Process: SummedFees not as expected");
            #endregion

            #region t3 TransactionType.Sell, "BTC", "EUR", 10.0m, 10.0m,"Test", "Test", 10.0m, 0.5m, "BTC"){ FeeValue = 0.5m}  --- After 1 year
            results[3] = transactions[3].Process(ref fsHelper, ref set);
            Assert.AreEqual(ProcessResult.FullParagraph23, results[3], "Process: Result[3] not as expected");
            asset0.CurrentAmount = 63.5m;
            asset0.CurrentValueOnBuyRate = 63.5m;
            HodledAsset asset3 = new HodledAsset("Test", "EUR", 10.0m, 10.0m, new DateTime(2001, 1, 1, 0, 0, 0));
            Assert.IsTrue(TestingExtension.ObjectsEquals(asset0, set.ElementAt(0), "Guid"), "Process: HodledAsset0 was not equal to expected");
            Assert.IsTrue(TestingExtension.ObjectsEquals(asset1, set.ElementAt(1), "Guid"), "Process: HodledAsset1 was not equal to expected");
            Assert.IsTrue(TestingExtension.ObjectsEquals(asset2, set.ElementAt(2), "Guid"), "Process: HodledAsset2 was not equal to expected");
            Assert.IsTrue(TestingExtension.ObjectsEquals(asset3, set.ElementAt(3), "Guid"), "Process: HodledAsset3 was not equal to expected");
            Assert.AreEqual(35.0m, fsHelper.First(x => x.Key == "Test").Value.summedSells, "Process: SummedSells not as expected");
            Assert.AreEqual(35.0m, fsHelper.First(x => x.Key == "Test").Value.summedBuys, "Process: SummedBuys not as expected");
            Assert.AreEqual(1.5m, fsHelper.First(x => x.Key == "Test").Value.summedFees, "Process: SummedFees not as expected");
            Assert.AreEqual(10.0m, fsHelper.First(x => x.Key == "Test").Value.summedSells23, "Process: SummedSells23 not as expected");
            Assert.AreEqual(10.0m, fsHelper.First(x => x.Key == "Test").Value.summedBuys23, "Process: SummedBuys23 not as expected");
            Assert.AreEqual(0.5m, fsHelper.First(x => x.Key == "Test").Value.summedFees23, "Process: SummedFees23 not as expected");
            #endregion

            #region t4 TransactionType.Sell, "BTC", "ETH", 70.0m, 70.0m,"Test", "Test", 70.0m, 70.0m, "USDT"){FeeValue = 2.0m} --- part (63.5) after 1 year, part (6.5) before 1 year
            results[4] = transactions[4].Process(ref fsHelper, ref set);
            Assert.AreEqual(ProcessResult.PartialParagraph23, results[4], "Process: Result[4] not as expected");
            asset0.CurrentAmount = 0.0m;
            asset0.CurrentValueOnBuyRate = 0.0m;
            asset0.removed = true;
            asset1.CurrentAmount = 13.0m; //fees
            asset1.CurrentValueOnBuyRate = 13.0m; //fees
            asset2.CurrentAmount = 3.0m;
            asset2.CurrentValueOnBuyRate = 3.0m;
            HodledAsset asset4 = new HodledAsset("Test", "ETH", 70.0m, 70.0m, new DateTime(2001, 12, 31, 23, 15, 0));
            Assert.IsTrue(TestingExtension.ObjectsEquals(asset0, set.ElementAt(0), "Guid"), "Process: HodledAsset0 was not equal to expected");
            Assert.IsTrue(TestingExtension.ObjectsEquals(asset1, set.ElementAt(1), "Guid"), "Process: HodledAsset1 was not equal to expected");
            Assert.IsTrue(TestingExtension.ObjectsEquals(asset2, set.ElementAt(2), "Guid"), "Process: HodledAsset2 was not equal to expected");
            Assert.IsTrue(TestingExtension.ObjectsEquals(asset3, set.ElementAt(3), "Guid"), "Process: HodledAsset3 was not equal to expected");
            Assert.IsTrue(TestingExtension.ObjectsEquals(asset4, set.ElementAt(4), "Guid"), "Process: HodledAsset4 was not equal to expected");
            Assert.AreEqual(41.5m, fsHelper.First(x => x.Key == "Test").Value.summedSells, "Process: SummedSells not as expected");
            Assert.AreEqual(41.5m, fsHelper.First(x => x.Key == "Test").Value.summedBuys, "Process: SummedBuys not as expected");
            Assert.AreEqual(1.69m, fsHelper.First(x => x.Key == "Test").Value.summedFees, "Process: SummedFees not as expected");
            Assert.AreEqual(73.5m, fsHelper.First(x => x.Key == "Test").Value.summedSells23, "Process: SummedSells23 not as expected");
            Assert.AreEqual(73.5m, fsHelper.First(x => x.Key == "Test").Value.summedBuys23, "Process: SummedBuys23 not as expected");
            Assert.AreEqual(2.31m, fsHelper.First(x => x.Key == "Test").Value.summedFees23, "Process: SummedFees23 not as expected");
            #endregion

            #region t5 TransactionType.Buy, "EUR", "BTC", 10.0m, 10.0m,"Test", "Test", 10.0m, 0.5m, "BTC"){FeeValue = 0.5m}
            results[5] = transactions[5].Process(ref fsHelper, ref set);
            Assert.AreEqual(ProcessResult.NoParagraph23, results[5], "Process: Result[5] not as expected");
            asset3.CurrentAmount = 0.0m;
            asset3.CurrentValueOnBuyRate = 0.0m;
            asset3.removed = true;
            HodledAsset asset5 = new HodledAsset("Test", "BTC", 10.0m, 10.0m, new DateTime(2001, 12, 31, 23, 20, 0))
            {
                CurrentAmount = 9.5m,
                CurrentValueOnBuyRate = 9.5m
            };
            Assert.IsTrue(TestingExtension.ObjectsEquals(asset0, set.ElementAt(0), "Guid"), "Process: HodledAsset0 was not equal to expected");
            Assert.IsTrue(TestingExtension.ObjectsEquals(asset1, set.ElementAt(1), "Guid"), "Process: HodledAsset1 was not equal to expected");
            Assert.IsTrue(TestingExtension.ObjectsEquals(asset2, set.ElementAt(2), "Guid"), "Process: HodledAsset2 was not equal to expected");
            Assert.IsTrue(TestingExtension.ObjectsEquals(asset3, set.ElementAt(3), "Guid"), "Process: HodledAsset3 was not equal to expected");
            Assert.IsTrue(TestingExtension.ObjectsEquals(asset4, set.ElementAt(4), "Guid"), "Process: HodledAsset4 was not equal to expected");
            Assert.IsTrue(TestingExtension.ObjectsEquals(asset5, set.ElementAt(5), "Guid"), "Process: HodledAsset5 was not equal to expected");
            Assert.AreEqual(41.5m, fsHelper.First(x => x.Key == "Test").Value.summedSells, "Process: SummedSells not as expected");
            Assert.AreEqual(41.5m, fsHelper.First(x => x.Key == "Test").Value.summedBuys, "Process: SummedBuys not as expected");
            Assert.AreEqual(2.19m, fsHelper.First(x => x.Key == "Test").Value.summedFees, "Process: SummedFees not as expected");
            Assert.AreEqual(73.5m, fsHelper.First(x => x.Key == "Test").Value.summedSells23, "Process: SummedSells23 not as expected");
            Assert.AreEqual(73.5m, fsHelper.First(x => x.Key == "Test").Value.summedBuys23, "Process: SummedBuys23 not as expected");
            Assert.AreEqual(2.31m, fsHelper.First(x => x.Key == "Test").Value.summedFees23, "Process: SummedFees23 not as expected");
            #endregion

            #region t6 TransactionType.Transfer, "BTC", "BTC", 7.0m, 7.0m, "Test", "Test2", 7.0m, 0.5m, "BTC"){FeeValue = 0.5m}
            results[6] = transactions[6].Process(ref fsHelper, ref set); //Withdrawal from Test
            results[7] = transactions[7].Process(ref fsHelper, ref set); //Deposit to Test2 (gets dropped)
            Assert.AreEqual(ProcessResult.NoParagraph23, results[6], "Process: Result[6] not as expected");
            Assert.AreEqual(ProcessResult.NoParagraph23, results[7], "Process: Result[7] not as expected");
            //a2 has 3 BTC, a5 has 9.5 BTC
            asset2.Wallet = "Test2";  //3 to Test2
            asset5.CurrentAmount = 5.0m; //4 to Test2, 0.5 fees
            asset5.CurrentValueOnBuyRate = 5.0m;
            HodledAsset asset6 = new HodledAsset("Test2", "BTC", 4.0m, 4.0m, asset5.Received);
            Assert.IsTrue(TestingExtension.ObjectsEquals(asset0, set.ElementAt(0), "Guid"), "Process: HodledAsset0 was not equal to expected");
            Assert.IsTrue(TestingExtension.ObjectsEquals(asset1, set.ElementAt(1), "Guid"), "Process: HodledAsset1 was not equal to expected");
            Assert.IsTrue(TestingExtension.ObjectsEquals(asset2, set.ElementAt(2), "Guid"), "Process: HodledAsset2 was not equal to expected");
            Assert.IsTrue(TestingExtension.ObjectsEquals(asset3, set.ElementAt(3), "Guid"), "Process: HodledAsset3 was not equal to expected");
            Assert.IsTrue(TestingExtension.ObjectsEquals(asset4, set.ElementAt(4), "Guid"), "Process: HodledAsset4 was not equal to expected");
            Assert.IsTrue(TestingExtension.ObjectsEquals(asset5, set.ElementAt(5), "Guid"), "Process: HodledAsset5 was not equal to expected");
            Assert.IsTrue(TestingExtension.ObjectsEquals(asset6, set.ElementAt(6), "Guid"), "Process: HodledAsset6 was not equal to expected");
            Assert.AreEqual(41.5m, fsHelper.First(x => x.Key == "Test").Value.summedSells, "Process: SummedSells not as expected");
            Assert.AreEqual(41.5m, fsHelper.First(x => x.Key == "Test").Value.summedBuys, "Process: SummedBuys not as expected");
            Assert.AreEqual(2.69m, fsHelper.First(x => x.Key == "Test").Value.summedFees, "Process: SummedFees not as expected");
            Assert.AreEqual(73.5m, fsHelper.First(x => x.Key == "Test").Value.summedSells23, "Process: SummedSells23 not as expected");
            Assert.AreEqual(73.5m, fsHelper.First(x => x.Key == "Test").Value.summedBuys23, "Process: SummedBuys23 not as expected");
            Assert.AreEqual(2.31m, fsHelper.First(x => x.Key == "Test").Value.summedFees23, "Process: SummedFees23 not as expected");
            #endregion

            #region t7 TransactionType.Transfer, "BTC", "BTC", 6.5m, 6.5m, "Test2", "Test", 65.0m, 0.5m, "BTC"){FeeValue = 0.5m}
            results[8] = transactions[8].Process(ref fsHelper, ref set); //Withdrawal from Test2
            results[9] = transactions[9].Process(ref fsHelper, ref set); //Deposit to Test (gets dropped)
            Assert.AreEqual(ProcessResult.NoParagraph23, results[8], "Process: Result[8] not as expected");
            Assert.AreEqual(ProcessResult.NoParagraph23, results[9], "Process: Result[9] not as expected");
            asset2.Wallet = "Test"; //3 to Test
            HodledAsset asset7 = new HodledAsset("Test", "BTC", 3.5m, 3.5m, asset6.Received);
            asset6.CurrentAmount = 0.0m; // -0.5 fee
            asset6.CurrentValueOnBuyRate = 0.0m;
            asset6.removed = true;
            //asset6 and 7 get switched in SortedSet due to Sorting
            HodledAsset temp = asset7;
            asset7 = asset6;
            asset6 = temp;
            Assert.IsTrue(TestingExtension.ObjectsEquals(asset0, set.ElementAt(0), "Guid"), "Process: HodledAsset0 was not equal to expected");
            Assert.IsTrue(TestingExtension.ObjectsEquals(asset1, set.ElementAt(1), "Guid"), "Process: HodledAsset1 was not equal to expected");
            Assert.IsTrue(TestingExtension.ObjectsEquals(asset2, set.ElementAt(2), "Guid"), "Process: HodledAsset2 was not equal to expected");
            Assert.IsTrue(TestingExtension.ObjectsEquals(asset3, set.ElementAt(3), "Guid"), "Process: HodledAsset3 was not equal to expected");
            Assert.IsTrue(TestingExtension.ObjectsEquals(asset4, set.ElementAt(4), "Guid"), "Process: HodledAsset4 was not equal to expected");
            Assert.IsTrue(TestingExtension.ObjectsEquals(asset5, set.ElementAt(5), "Guid"), "Process: HodledAsset5 was not equal to expected");
            Assert.IsTrue(TestingExtension.ObjectsEquals(asset6, set.ElementAt(6), "Guid"), "Process: HodledAsset6 was not equal to expected");
            Assert.IsTrue(TestingExtension.ObjectsEquals(asset7, set.ElementAt(7), "Guid"), "Process: HodledAsset7 was not equal to expected");
            Assert.AreEqual(41.5m, fsHelper.First(x => x.Key == "Test").Value.summedSells, "Process: SummedSells not as expected");
            Assert.AreEqual(41.5m, fsHelper.First(x => x.Key == "Test").Value.summedBuys, "Process: SummedBuys not as expected");
            Assert.AreEqual(2.69m, fsHelper.First(x => x.Key == "Test").Value.summedFees, "Process: SummedFees not as expected");
            Assert.AreEqual(73.5m, fsHelper.First(x => x.Key == "Test").Value.summedSells23, "Process: SummedSells23 not as expected");
            Assert.AreEqual(73.5m, fsHelper.First(x => x.Key == "Test").Value.summedBuys23, "Process: SummedBuys23 not as expected");
            Assert.AreEqual(2.31m, fsHelper.First(x => x.Key == "Test").Value.summedFees23, "Process: SummedFees23 not as expected");
            //Test2
            Assert.AreEqual(0.5m, fsHelper.First(x => x.Key == "Test2").Value.summedFees, "Process: SummedFees not as expected");
            #endregion

            #region t8 TransactionType.Distribution, "TRX", "TRX", 10.0m, 0.0m,"Test", "Test", 10.0m, 0.0m, string.Empty)
            results[10] = transactions[10].Process(ref fsHelper, ref set);
            Assert.AreEqual(ProcessResult.NoParagraph23, results[10], "Process: Result[10] not as expected");
            HodledAsset asset8 = new HodledAsset("Test", "TRX", 10.0m, 0.0m, new DateTime(2002, 1, 2, 0, 0, 0)); //distribution have always value 0
            Assert.IsTrue(TestingExtension.ObjectsEquals(asset0, set.ElementAt(0), "Guid"), "Process: HodledAsset0 was not equal to expected");
            Assert.IsTrue(TestingExtension.ObjectsEquals(asset1, set.ElementAt(1), "Guid"), "Process: HodledAsset1 was not equal to expected");
            Assert.IsTrue(TestingExtension.ObjectsEquals(asset2, set.ElementAt(2), "Guid"), "Process: HodledAsset2 was not equal to expected");
            Assert.IsTrue(TestingExtension.ObjectsEquals(asset3, set.ElementAt(3), "Guid"), "Process: HodledAsset3 was not equal to expected");
            Assert.IsTrue(TestingExtension.ObjectsEquals(asset4, set.ElementAt(4), "Guid"), "Process: HodledAsset4 was not equal to expected");
            Assert.IsTrue(TestingExtension.ObjectsEquals(asset5, set.ElementAt(5), "Guid"), "Process: HodledAsset5 was not equal to expected");
            Assert.IsTrue(TestingExtension.ObjectsEquals(asset6, set.ElementAt(6), "Guid"), "Process: HodledAsset6 was not equal to expected");
            Assert.IsTrue(TestingExtension.ObjectsEquals(asset7, set.ElementAt(7), "Guid"), "Process: HodledAsset7 was not equal to expected");
            Assert.IsTrue(TestingExtension.ObjectsEquals(asset8, set.ElementAt(8), "Guid"), "Process: HodledAsset8 was not equal to expected");
            Assert.AreEqual(41.5m, fsHelper.First(x => x.Key == "Test").Value.summedSells, "Process: SummedSells not as expected");
            Assert.AreEqual(41.5m, fsHelper.First(x => x.Key == "Test").Value.summedBuys, "Process: SummedBuys not as expected");
            Assert.AreEqual(2.69m, fsHelper.First(x => x.Key == "Test").Value.summedFees, "Process: SummedFees not as expected");
            Assert.AreEqual(73.5m, fsHelper.First(x => x.Key == "Test").Value.summedSells23, "Process: SummedSells23 not as expected");
            Assert.AreEqual(73.5m, fsHelper.First(x => x.Key == "Test").Value.summedBuys23, "Process: SummedBuys23 not as expected");
            Assert.AreEqual(2.31m, fsHelper.First(x => x.Key == "Test").Value.summedFees23, "Process: SummedFees23 not as expected");
            //Test2
            Assert.AreEqual(0.5m, fsHelper.First(x => x.Key == "Test2").Value.summedFees, "Process: SummedFees not as expected");
            #endregion

            #region t9 TransactionType.Sell, "TRX", "USDT", 10.0m, 50.0m,"Test", "Test", 50.0m, 5.0m, "USDT")  {FeeValue = 5.0m }
            results[11] = transactions[11].Process(ref fsHelper, ref set);
            Assert.AreEqual(ProcessResult.NoParagraph23, results[11], "Process: Result[11] not as expected");
            asset8.CurrentAmount = 0.0m;
            asset8.CurrentValueOnBuyRate = 0.0m;
            asset8.removed = true;
            HodledAsset asset9 = new HodledAsset("Test", "USDT", 50.0m, 50.0m, new DateTime(2002, 1, 3, 0, 0, 0))
            {
                CurrentAmount = 45.0m,
                CurrentValueOnBuyRate = 45.0m
            };
            Assert.IsTrue(TestingExtension.ObjectsEquals(asset0, set.ElementAt(0), "Guid"), "Process: HodledAsset0 was not equal to expected");
            Assert.IsTrue(TestingExtension.ObjectsEquals(asset1, set.ElementAt(1), "Guid"), "Process: HodledAsset1 was not equal to expected");
            Assert.IsTrue(TestingExtension.ObjectsEquals(asset2, set.ElementAt(2), "Guid"), "Process: HodledAsset2 was not equal to expected");
            Assert.IsTrue(TestingExtension.ObjectsEquals(asset3, set.ElementAt(3), "Guid"), "Process: HodledAsset3 was not equal to expected");
            Assert.IsTrue(TestingExtension.ObjectsEquals(asset4, set.ElementAt(4), "Guid"), "Process: HodledAsset4 was not equal to expected");
            Assert.IsTrue(TestingExtension.ObjectsEquals(asset5, set.ElementAt(5), "Guid"), "Process: HodledAsset5 was not equal to expected");
            Assert.IsTrue(TestingExtension.ObjectsEquals(asset6, set.ElementAt(6), "Guid"), "Process: HodledAsset6 was not equal to expected");
            Assert.IsTrue(TestingExtension.ObjectsEquals(asset7, set.ElementAt(7), "Guid"), "Process: HodledAsset7 was not equal to expected");
            Assert.IsTrue(TestingExtension.ObjectsEquals(asset8, set.ElementAt(8), "Guid"), "Process: HodledAsset8 was not equal to expected");
            Assert.IsTrue(TestingExtension.ObjectsEquals(asset9, set.ElementAt(9), "Guid"), "Process: HodledAsset9 was not equal to expected");
            Assert.AreEqual(91.5m, fsHelper.First(x => x.Key == "Test").Value.summedSells, "Process: SummedSells not as expected");
            Assert.AreEqual(41.5m, fsHelper.First(x => x.Key == "Test").Value.summedBuys, "Process: SummedBuys not as expected");
            Assert.AreEqual(7.69m, fsHelper.First(x => x.Key == "Test").Value.summedFees, "Process: SummedFees not as expected");
            Assert.AreEqual(73.5m, fsHelper.First(x => x.Key == "Test").Value.summedSells23, "Process: SummedSells23 not as expected");
            Assert.AreEqual(73.5m, fsHelper.First(x => x.Key == "Test").Value.summedBuys23, "Process: SummedBuys23 not as expected");
            Assert.AreEqual(2.31m, fsHelper.First(x => x.Key == "Test").Value.summedFees23, "Process: SummedFees23 not as expected");
            //Test2
            Assert.AreEqual(0.5m, fsHelper.First(x => x.Key == "Test2").Value.summedFees, "Process: SummedFees not as expected");
            #endregion
        }

        /// <summary>
        /// This method processes a transaction with regard to the update of the HodledAssets and the AnnualFinancialStatements (including §23 EStG)
        /// </summary>
        /// <param name="transaction">The transaction, which needs to process</param>
        /// <param name="afsHelper">Reference to the Annual Financial Statement Helper</param>
        /// <param name="hodledAssets">Reference to the HodledAssets Set</param>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "HodledAsset did not hold an asset, but did allow to sell it.")]
        public void ProcessSellWithException()
        {
            Dictionary<string, FinancialStatementHelper> fsHelper = new Dictionary<string, FinancialStatementHelper>();
            SortedSet<HodledAsset> set = new SortedSet<HodledAsset>();
            new Transaction("Test", DateTime.UtcNow, TransactionType.Sell, "BTC", "EXC", 1000.0m, 1.0m, "Test", "Test", 1000.0m, 1.0m, "EUR", 1.0m).Process(ref fsHelper, ref set); //This should result in an invalidoperation-exception
        }

        /// <summary>
        /// This method processes a transaction with regard to the update of the HodledAssets and the AnnualFinancialStatements (including §23 EStG)
        /// </summary>
        /// <param name="transaction">The transaction, which needs to process</param>
        /// <param name="afsHelper">Reference to the Annual Financial Statement Helper</param>
        /// <param name="hodledAssets">Reference to the HodledAssets Set</param>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "HodledAsset did not hold an asset, but did allow to withdraw it.")]
        public void ProcessWithdrawWithException()
        {
            Dictionary<string, FinancialStatementHelper> fsHelper = new Dictionary<string, FinancialStatementHelper>();
            SortedSet<HodledAsset> set = new SortedSet<HodledAsset>();
            new Transaction("Test", DateTime.UtcNow, TransactionType.BankWithdrawal, "EUR", "EUR", 1000.0m, 1000.0m, "Test", "Bank", 1000.0m, 1.0m, "EUR", 1.0m).Process(ref fsHelper, ref set); //This should result in an invalidoperation-exception
        }

        /// <summary>
        /// This method processes a transaction with regard to the update of the HodledAssets and the AnnualFinancialStatements (including §23 EStG)
        /// </summary>
        /// <param name="transaction">The transaction, which needs to process</param>
        /// <param name="afsHelper">Reference to the Annual Financial Statement Helper</param>
        /// <param name="hodledAssets">Reference to the HodledAssets Set</param>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "HodledAsset did not hold an asset, but did allow to transfer it.")]
        public void ProcessTransferWithException()
        {
            Dictionary<string, FinancialStatementHelper> fsHelper = new Dictionary<string, FinancialStatementHelper>();
            SortedSet<HodledAsset> set = new SortedSet<HodledAsset>();
            new Transaction("Test", DateTime.UtcNow, TransactionType.Transfer, "BTC", "BTC", 1000.0m, 1000.0m, "Test", "Test2", 1000.0m, 1.0m, "EUR", 1.0m).Process(ref fsHelper, ref set); //This should result in an invalidoperation-exception
        }
    }
}