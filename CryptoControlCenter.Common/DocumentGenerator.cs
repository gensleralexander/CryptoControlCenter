using CryptoControlCenter.Common.Database;
using CryptoControlCenter.Common.Enums;
using CryptoControlCenter.Common.Helper;
using CryptoControlCenter.Common.Models;
using CryptoControlCenter.Common.Models.Interfaces;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CryptoControlCenter.Common
{
    /// <summary>
    /// This class is responsible for the creation of documents
    /// </summary>
    public static class DocumentGenerator
    {
        public static async Task<string> GenerateLogs()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var logFile = new FileInfo(Path.Combine(path, "CryptoControlCenter.log"));
            List<LogEntry> logs = await SQLiteDatabaseManager.Database.Table<LogEntry>().ToListAsync();
            try
            {
                using (StreamWriter w = File.AppendText(logFile.FullName))
                {
                    foreach (LogEntry entry in logs)
                    {
                        try
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.Append(entry.DateTime.ToString("dd-MMM-yyyy - HH:mm:ss"));
                            sb.Append(" --- ");
                            if (entry.Trigger != null)
                            {
                                sb.Append(entry.Trigger);
                                sb.Append(" --- ");
                            }
                            sb.Append(entry.Message);
                            w.WriteLine(sb.ToString());
                        }
                        catch (Exception) { }
                    }
                }
            }
            catch (Exception) { }
            return logFile.FullName;
        }

        /// <summary>
        /// Creates the crypto tax report (currently only german) for a given time period.
        /// </summary>
        /// <param name="toYear">Ending year. If NULL, then last year is used.</param>
        /// <param name="language">Language for excel sheet.</param>
        /// <exception cref="InvalidOperationException">Occurs, when there is no valid worksheet found for Location.</exception>
        public static async Task<string> GenerateCryptoTaxReport(int? toYear = null, string language = "en")
        {
            DateTime to;
            if (toYear == null || toYear > DateTime.UtcNow.Year) //default is until end of last year
            {
                to = new DateTime(DateTime.UtcNow.Year - 1, 12, 31, 23, 59, 59);
            }
            else
            {
                to = new DateTime((int)toYear, 12, 31, 23, 59, 59);
            }
            //Load entries from database, create Dictionary for helpers and split them by PK WalletNames (=own worksheets)
            List<ExchangeWallet> exchWallets = await SQLiteDatabaseManager.Database.Table<ExchangeWallet>().ToListAsync();
            List<Transaction> transactions = await SQLiteDatabaseManager.Database.Table<Transaction>().Where(x => x.TransactionTime <= to).ToListAsync();
            if (transactions.Count == 0)
            {
                return "No transactions found for specified time range. Please check your parameters";
            }
            else
            {
                transactions.Sort();
                Dictionary<string, FinancialStatementHelper> fsDictionary = new Dictionary<string, FinancialStatementHelper>();
                List<string> wallets = new List<string>();
                wallets.AddRange(transactions.Select(x => x.LocationStart));
                wallets.AddRange(transactions.Select(x => x.LocationDestination));
                foreach (string walletname in wallets.Distinct())
                {
                    if (!string.IsNullOrWhiteSpace(walletname) && walletname != "Bank" && walletname != "Unbekanntes Wallet")
                    {
                        fsDictionary.Add(walletname, new FinancialStatementHelper(2, 0));
                    }
                }
                SortedSet<HodledAsset> hodledAssets = new SortedSet<HodledAsset>();
                ITransactionViewer last = null;
                ExcelWorksheet worksheet;

                //Setup License and ExcelPackage
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var xlFile = new FileInfo(Path.Combine(path, "TaxReport_" + DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss") + ".xlsx"));
                using (var package = new ExcelPackage())
                {
                    // Create Worksheets and headers
                    foreach (KeyValuePair<string, FinancialStatementHelper> exchange in fsDictionary)
                    {
                        worksheet = package.Workbook.Worksheets.Add(exchange.Key);
                        worksheet.Cells[1, 1].Value = "Jahr";
                        worksheet.Cells[1, 2].Value = "Lfd. Nr.";
                        worksheet.Cells[1, 3].Value = "Datum";
                        worksheet.Cells[1, 4].Value = "Uhrzeit";
                        worksheet.Cells[1, 5].Value = "Transaktionsart";
                        worksheet.Cells[1, 6].Value = "Start-Wallet / Plattform";
                        worksheet.Cells[1, 7].Value = "Ziel-Wallet / Tausch-Paar";
                        worksheet.Cells[1, 8].Value = "Menge Coins";
                        worksheet.Cells[1, 9].Value = "Währung";
                        worksheet.Cells[1, 10].Value = "Wechselkurs (gerundet)";
                        worksheet.Cells[1, 11].Value = "Kurs zum Zeitpunkt der Transaktion";
                        worksheet.Cells[1, 12].Value = "Wert der Coins";
                        worksheet.Cells[1, 13].Value = "Gebühr Krypto";
                        worksheet.Cells[1, 14].Value = "Gebühr (gerundet)";
                        worksheet.Cells[1, 15].Value = "Bemerkungen";
                        worksheet.Cells[1, 1, 1, 15].Style.Font.Bold = true;

                        worksheet.Cells[3, 1].Value = transactions.First(x => x.Wallet == exchange.Key).TransactionTime.Year;
                        worksheet.Cells[3, 1].Style.Font.Bold = true;
                        worksheet.Cells[3, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[3, 1].Style.Fill.BackgroundColor.SetColor(ExcelIndexedColor.Indexed47);//Excel Orange
                    }
#if DEBUG
                    int debugLfd = 0;
#endif
                    //Process transactions
                    foreach (ITransactionViewer transaction in transactions)
                    {
                        if (transaction.TransferValue > 0.0099m || transaction.TransactionType == TransactionType.Distribution)
                        {
#if DEBUG
                            Console.Write(debugLfd + " --- ");
                            Console.WriteLine(transaction.ToString());
                            debugLfd++;
#endif
                            //Check for a change in year -> Annual Financial Statement
                            if (last != null && transaction.TransactionTime.Year > last.TransactionTime.Year)
                            {
                                foreach (KeyValuePair<string, FinancialStatementHelper> exchange in fsDictionary)
                                {
                                    if (exchange.Value.processedTransactions > 0)
                                    {
                                        worksheet = package.Workbook.Worksheets.First(x => x.Name == exchange.Key);
                                        insertAnnualFinancialStatements(ref worksheet, last.TransactionTime.Year, exchange.Value);
                                        exchange.Value.currentRow++;
                                        worksheet.Cells[exchange.Value.currentRow, 1].Value = transaction.TransactionTime.Year;
                                        worksheet.Cells[exchange.Value.currentRow, 1].Style.Font.Bold = true;
                                        worksheet.Cells[exchange.Value.currentRow, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                        worksheet.Cells[exchange.Value.currentRow, 1].Style.Fill.BackgroundColor.SetColor(ExcelIndexedColor.Indexed47);//Excel Orange
                                        exchange.Value.currentRow++;
                                    }
                                }
                            }
                            try
                            {
                                worksheet = package.Workbook.Worksheets.First(x => x.Name == transaction.Wallet);
                            }
                            catch
                            {
                                throw new InvalidOperationException("No valid worksheet found for Location: " + transaction.Wallet);
                            }
                            fsDictionary.First(x => x.Key == worksheet.Name).Value.currentRow++;
                            fsDictionary.First(x => x.Key == worksheet.Name).Value.currentNr++;
                            int currentRow = fsDictionary.First(x => x.Key == worksheet.Name).Value.currentRow;
                            //Process transaction
#if DEBUG
                            ProcessResult result = ProcessResult.NoParagraph23;
                            try
                            {
                                result = transaction.Process(ref fsDictionary, ref hodledAssets);
                            }
                            catch
                            {
                                Console.WriteLine("Error occured during processing of transactions");
                                Console.WriteLine("Saving Excel Sheet for debugging purpose...");
                                package.SaveAs(xlFile);
                                break;
                            }
#else
                            ProcessResult result = transaction.Process(ref fsDictionary, ref hodledAssets);
#endif
                            fsDictionary.First(x => x.Key == worksheet.Name).Value.processedTransactions++;
                            //Insert common values
                            worksheet.Cells[currentRow, 2].Value = fsDictionary.First(x => x.Key == worksheet.Name).Value.currentNr;
                            worksheet.Cells[currentRow, 3].Style.Numberformat.Format = "dd.MM.yyyy";
                            worksheet.Cells[currentRow, 3].Value = transaction.TransactionTime.Date;
                            worksheet.Cells[currentRow, 4].Style.Numberformat.Format = "hh:mm:ss";
                            worksheet.Cells[currentRow, 4].Value = transaction.TransactionTime.TimeOfDay;
                            worksheet.Cells[currentRow, 8].Style.Numberformat.Format = "#,##0.00000000";
                            worksheet.Cells[currentRow, 8].Value = transaction.AmountStart;
                            worksheet.Cells[currentRow, 9].Value = transaction.AssetStart;
                            worksheet.Cells[currentRow, 11].Style.Numberformat.Format = "#,##0.00 €";
                            worksheet.Cells[currentRow, 12].Style.Numberformat.Format = "#,##0.00 €";

                            if (result == ProcessResult.FullParagraph23)
                            {
                                worksheet.Cells[currentRow, 15].Value = "Komplette Menge " + transaction.AssetStart + " mit Haltedauer > 1 Jahr";
                            }
                            else if (result == ProcessResult.PartialParagraph23)
                            {
                                worksheet.Cells[currentRow, 15].Value = "Teilmenge " + transaction.AssetStart + " mit Haltedauer > 1 Jahr";
                            }

                            switch (transaction.TransactionType)
                            {
                                case TransactionType.Buy:
                                case TransactionType.Sell:
                                    worksheet.Cells[currentRow, 6].Value = transaction.LocationStart;
                                    worksheet.Cells[currentRow, 7].Value = transaction.GetTradingPair();
                                    worksheet.Cells[currentRow + 1, 8].Style.Numberformat.Format = "#,##0.00000000";
                                    worksheet.Cells[currentRow + 1, 8].Value = transaction.AmountDestination;
                                    worksheet.Cells[currentRow + 1, 9].Value = transaction.AssetDestination;
                                    worksheet.Cells[currentRow, 10].Value = transaction.GetExchangeRateString();
                                    worksheet.Cells[currentRow + 1, 11].Style.Numberformat.Format = "#,##0.00 €";
                                    worksheet.Cells[currentRow, 12].Value = transaction.TransferValue;
                                    worksheet.Cells[currentRow + 1, 12].Style.Numberformat.Format = "#,##0.00 €";
                                    worksheet.Cells[currentRow + 1, 12].Value = transaction.TransferValue;
                                    worksheet.Cells[currentRow, 13].Value = transaction.FeeAmount + " " + transaction.FeeAsset;
                                    worksheet.Cells[currentRow, 13].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                                    worksheet.Cells[currentRow, 14].Style.Numberformat.Format = "#,##0.00 €";
                                    worksheet.Cells[currentRow, 14].Value = transaction.FeeValue;
                                    if (transaction.AssetStart.IsFIATcurrency())
                                    {
                                        worksheet.Cells[currentRow, 5].Value = "Kauf";
                                        //worksheet.Cells[currentRow, 11].Value = 1.0m;
                                        worksheet.Cells[currentRow + 1, 11].Value = transaction.TransferValue / transaction.AmountDestination;

                                    }
                                    else if (transaction.AssetDestination.IsFIATcurrency())
                                    {
                                        worksheet.Cells[currentRow, 5].Value = "Verkauf";
                                        worksheet.Cells[currentRow, 11].Value = transaction.TransferValue / transaction.AmountStart;
                                        //worksheet.Cells[currentRow + 1, 11].Value = 1.0m;

                                    }
                                    else
                                    {
                                        worksheet.Cells[currentRow, 5].Value = "Tausch";
                                        worksheet.Cells[currentRow, 11].Value = transaction.TransferValue / transaction.AmountStart;
                                        worksheet.Cells[currentRow + 1, 11].Value = transaction.TransferValue / transaction.AmountDestination;
                                    }
                                    currentRow++;
                                    break;
                                case TransactionType.Transfer:
                                    worksheet.Cells[currentRow, 5].Value = "Transfer";
                                    worksheet.Cells[currentRow, 6].Value = transaction.LocationStart;
                                    worksheet.Cells[currentRow, 7].Value = transaction.LocationDestination;
                                    worksheet.Cells[currentRow, 11].Value = transaction.TransferValue / transaction.AmountStart;
                                    worksheet.Cells[currentRow, 12].Value = transaction.TransferValue;
                                    break;
                                case TransactionType.BankDeposit:
                                    worksheet.Cells[currentRow, 5].Value = "Einzahlung";
                                    worksheet.Cells[currentRow, 6].Value = "Bank";
                                    worksheet.Cells[currentRow, 7].Value = transaction.LocationDestination;
                                    //worksheet.Cells[currentRow, 11].Value = transaction.TransferValue / transaction.AmountStart;
                                    worksheet.Cells[currentRow, 12].Value = transaction.TransferValue;
                                    break;
                                case TransactionType.BankWithdrawal:
                                    worksheet.Cells[currentRow, 5].Value = "Auszahlung";
                                    worksheet.Cells[currentRow, 6].Value = transaction.LocationStart;
                                    worksheet.Cells[currentRow, 7].Value = "Bank";
                                    //worksheet.Cells[currentRow, 11].Value = transaction.TransferValue / transaction.AmountStart;
                                    worksheet.Cells[currentRow, 12].Value = transaction.TransferValue;
                                    break;
                                case TransactionType.Distribution:
                                    worksheet.Cells[currentRow, 5].Value = "Distribution";
                                    worksheet.Cells[currentRow, 12].Value = 0.00m;
                                    worksheet.Cells[currentRow, 15].Value = "Distributionen werden bei Veräußerung mit Anschaffungskosten 0€ verrechnet.";
                                    break;
                                case TransactionType.Dust:
                                    break;
                            }
                            fsDictionary.First(x => x.Key == worksheet.Name).Value.currentRow++;
                        }
                        last = transaction;
                    }
                    //End of transaction loop

                    //insert one last yearly financial statement
                    foreach (KeyValuePair<string, FinancialStatementHelper> exchange in fsDictionary)
                    {
                        worksheet = package.Workbook.Worksheets.First(x => x.Name == exchange.Key);
                        insertAnnualFinancialStatements(ref worksheet, transactions.Last().TransactionTime.Year, exchange.Value);
                    }

                    //Auto-Width all columns
                    foreach (ExcelWorksheet ws in package.Workbook.Worksheets)
                    {
                        ws.Cells[ws.Dimension.Address].AutoFitColumns();
                    }
                    // Save the workbook in the output directory and return path to it
                    package.SaveAs(xlFile);
                    return xlFile.FullName;
                }
            }
        }

        /// <summary>
        /// Inserts the Annual Financial Statement
        /// </summary>
        /// <param name="worksheet">Reference to the ExcelWorksheet</param>
        /// <param name="currentRow">Reference to the currentRow</param>
        /// <param name="year">Reference to the currentYear</param>
        /// <param name="afsHelper">Reference to the AnnualFinancialStatementHelper</param>
        private static void insertAnnualFinancialStatements(ref ExcelWorksheet worksheet, int year, FinancialStatementHelper fsHelper)
        {
            fsHelper.currentRow++;
            worksheet.Cells[fsHelper.currentRow, 1, fsHelper.currentRow, 14].Style.Border.Top.Style = ExcelBorderStyle.Double;
            fsHelper.currentRow++;
            worksheet.Cells[fsHelper.currentRow, 1, fsHelper.currentRow, 3].Value = "Jahresabschluss " + year;
            worksheet.Cells[fsHelper.currentRow, 1, fsHelper.currentRow, 3].Merge = true;
            worksheet.Cells[fsHelper.currentRow, 1, fsHelper.currentRow, 3].Style.Font.Bold = true;
            worksheet.Cells[fsHelper.currentRow, 1, fsHelper.currentRow, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[fsHelper.currentRow, 1, fsHelper.currentRow, 3].Style.Fill.BackgroundColor.SetColor(ExcelIndexedColor.Indexed47);//Excel Orange
            //General Style for Cells
            worksheet.Cells[fsHelper.currentRow, 12, fsHelper.currentRow + 4, 12].Style.Numberformat.Format = "#,##0.00\" € \";\"-\"#,##0.00\" € \";\" -\"#\" € \";@\" \"";
            worksheet.Cells[fsHelper.currentRow, 11, fsHelper.currentRow + 4, 12].Style.Font.Bold = true;
            worksheet.Cells[fsHelper.currentRow, 11, fsHelper.currentRow + 4, 12].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            //Set Values
            worksheet.Cells[fsHelper.currentRow, 11].Value = "Veräußerungspreis (gesamt)";
            worksheet.Cells[fsHelper.currentRow, 12].Value = fsHelper.summedSells;
            fsHelper.currentRow++;
            worksheet.Cells[fsHelper.currentRow, 11].Value = "- Anschaffungskosten (gesamt)";
            worksheet.Cells[fsHelper.currentRow, 12].Value = fsHelper.summedBuys;
            fsHelper.currentRow++;
            worksheet.Cells[fsHelper.currentRow, 11].Value = "- Werbungskosten (gesamt)";
            worksheet.Cells[fsHelper.currentRow, 12].Value = fsHelper.summedFees;
            fsHelper.currentRow++;
            worksheet.Cells[fsHelper.currentRow, 11].Value = "Gewinn/Verlust" + year + "(gesamt)";
            worksheet.Cells[fsHelper.currentRow, 12].Value = fsHelper.summedSells - fsHelper.summedBuys - fsHelper.summedFees;
            worksheet.Cells[fsHelper.currentRow, 11, fsHelper.currentRow, 12].Style.Border.Top.Style = ExcelBorderStyle.Medium;
            //Conditional Format for Positive/Negative
            var greater = worksheet.Cells[fsHelper.currentRow, 12].ConditionalFormatting.AddGreaterThan();
            var less = worksheet.Cells[fsHelper.currentRow, 12].ConditionalFormatting.AddLessThan();
            greater.Formula = "0";
            less.Formula = "0";
            greater.Style.Fill.PatternType = ExcelFillStyle.Solid;
            less.Style.Fill.PatternType = ExcelFillStyle.Solid;
            greater.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(0, 198, 239, 206));
            less.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(0, 255, 199, 206));
            fsHelper.currentRow++;

            if (fsHelper.paragraph23estg)    //Occurs if at least one asset was hodl'ed longer than one year
            {
                fsHelper.currentRow++;
                worksheet.Cells[fsHelper.currentRow, 11].Value = "Steuerfreie Transaktionen, da Haltedauer gemäß §23 EStG über ein Jahr";
                worksheet.Cells[fsHelper.currentRow, 11].Style.Font.Bold = true;
                worksheet.Cells[fsHelper.currentRow, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                fsHelper.currentRow++;
                //General Style for Cells
                worksheet.Cells[fsHelper.currentRow, 12, fsHelper.currentRow + 4, 12].Style.Numberformat.Format = "#,##0.00\" € \";\"-\"#,##0.00\" € \";\" -\"#\" € \";@\" \"";
                worksheet.Cells[fsHelper.currentRow, 11, fsHelper.currentRow + 4, 12].Style.Font.Bold = true;
                worksheet.Cells[fsHelper.currentRow, 11, fsHelper.currentRow + 4, 12].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                //Set Values
                worksheet.Cells[fsHelper.currentRow, 11].Value = "Veräußerungspreis (gesamt)";
                worksheet.Cells[fsHelper.currentRow, 12].Value = fsHelper.summedSells23;
                fsHelper.currentRow++;
                worksheet.Cells[fsHelper.currentRow, 11].Value = "- Anschaffungskosten (gesamt)";
                worksheet.Cells[fsHelper.currentRow, 12].Value = fsHelper.summedBuys23;
                fsHelper.currentRow++;
                worksheet.Cells[fsHelper.currentRow, 11].Value = "- Werbungskosten (gesamt)";
                worksheet.Cells[fsHelper.currentRow, 12].Value = fsHelper.summedFees23;
                fsHelper.currentRow++;
                worksheet.Cells[fsHelper.currentRow, 11].Value = "Gewinn/Verlust" + year + "(gesamt)";
                worksheet.Cells[fsHelper.currentRow, 12].Value = fsHelper.summedSells23 - fsHelper.summedBuys23 - fsHelper.summedFees23;
                worksheet.Cells[fsHelper.currentRow, 11, fsHelper.currentRow, 12].Style.Border.Top.Style = ExcelBorderStyle.Medium;
                //Conditional Format for Positive/Negative
                var greater2 = worksheet.Cells[fsHelper.currentRow, 12].ConditionalFormatting.AddGreaterThan();
                var less2 = worksheet.Cells[fsHelper.currentRow, 12].ConditionalFormatting.AddLessThan();
                greater2.Formula = "0";
                less2.Formula = "0";
                greater2.Style.Fill.PatternType = ExcelFillStyle.Solid;
                less2.Style.Fill.PatternType = ExcelFillStyle.Solid;
                greater2.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(0, 198, 239, 206));
                less2.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(0, 255, 199, 206));
                fsHelper.currentRow++;
            }
            //Insert fsHelper.current balances
            fsHelper.currentRow++;

            //After inserting the annual financial statement, clear the helper, so the next year can be processed
            fsHelper.InitializeYear();
        }
    }
}