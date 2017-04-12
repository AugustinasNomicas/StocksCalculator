using System;
using System.Collections.Generic;
using System.Text;

namespace StocksCalculator.Models.PortfolioCalculator
{
    public class CalculateRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string StocksTicker { get; set; }
        public string BondsTicker { get; set; }

        public decimal StartMoneyAmount { get; set; }
        public decimal TransactionFee { get; set; }
    }

    public class CalculateMonthResult
    {
        public DateTime Date { get; set; }
        public decimal StocksRation { get; set; }
        public decimal BondsRation { get; set; }
        public int StocksDelta { get; set; }
        public int BondsDelta { get; set; }
    }

    public class CalculatResult
    {
        public List<CalculateMonthResult> MonthResults { get; set; } = new List<CalculateMonthResult>();
        public decimal AverageYearReturn { get; set; }
        public decimal AverageYearReturnAfterFee { get; set; }
        public decimal TotalMoneyAmount { get; set; }
        public decimal TotalMoneyAmountAfterFee { get; set; }
        public decimal TotalTransactionFee { get; set; }
    }
}
