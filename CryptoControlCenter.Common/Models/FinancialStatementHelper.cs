using System.Text;

namespace CryptoControlCenter.Common.Models
{
    /// <summary>
    /// This is a helper model for TaxReport.
    /// </summary>
    public class FinancialStatementHelper
    {
        public decimal summedSells { get; set; }
        public decimal summedBuys { get; set; }
        public decimal summedFees { get; set; }

        public decimal summedSells23 { get; set; }
        public decimal summedBuys23 { get; set; }
        public decimal summedFees23 { get; set; }

        public bool paragraph23estg { get; set; }

        public int currentRow { get; set; }
        public int currentNr { get; set; }
        public int processedTransactions { get; set; }

        public FinancialStatementHelper(int _currentRow, int _currentNr)
        {
            InitializeYear();
            currentRow = _currentRow;
            currentNr = _currentNr;
        }

        public void InitializeYear()
        {
            summedSells = 0.0m;
            summedBuys = 0.0m;
            summedFees = 0.0m;
            summedSells23 = 0.0m;
            summedBuys23 = 0.0m;
            summedFees23 = 0.0m;
            paragraph23estg = false;
            processedTransactions = 0;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Buys: ");
            sb.AppendLine(summedBuys.ToString());
            sb.Append("Buys23: ");
            sb.AppendLine(summedBuys23.ToString());
            sb.Append("Sells: ");
            sb.AppendLine(summedSells.ToString());
            sb.Append("Sells23: ");
            sb.AppendLine(summedSells23.ToString());
            sb.Append("Fees: ");
            sb.AppendLine(summedFees.ToString());
            sb.Append("Fees23: ");
            sb.AppendLine(summedFees23.ToString());
            return sb.ToString();
        }
    }
}
