using System;
using System.Collections.Generic;

namespace StocksCalculator.Models.PortfolioCalculator
{
    public class CalculateRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string StocksTicker { get; set; }
        public string BondsTicker { get; set; }
    }

    public class CalculateMonthResult
    {
        public DateTime Date { get; set; }
        public decimal StocksRatio { get; set; }
        public decimal BondsRatio { get; set; }
        public decimal StockPrice { get; set; }
        public decimal BondPrice { get; set; }

        public int StocksInPortfolio { get; set; }
        public int BondsInPortfolio { get; set; }

        public decimal ValueInCash { get; set; }
        public decimal ValueInCashAfterFee { get; set; }
        public decimal TransactionFee { get; set; }
    }

    public class PerformanceResults
    {
        public List<CalculateMonthResult> MonthResults { get; set; } = new List<CalculateMonthResult>();
        public decimal TotalMoneyAmount { get; set; }
        public decimal TotalMoneyAmountAfterFee { get; set; }
        public decimal TotalTransactionFee { get; set; }
    }
}
