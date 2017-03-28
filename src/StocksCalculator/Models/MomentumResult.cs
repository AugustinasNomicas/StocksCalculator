using System;

namespace StocksCalculator.Models
{
    public class MomentumResult : IStrategyResult
    {
        public DateTime Date { get; set; }

        public decimal Stocks12MonthMovingAverage { get; set; }
        public decimal Stocks3MonthMom { get; set; }
        public decimal Stocks6MonthMom { get; set; }
        public decimal Stocks12MonthMom { get; set; }
        public decimal StocksAverageMomentum { get; set; }
        public bool TffFilter { get; set; }

        public decimal Bonds3MonthMom { get; set; }
        public decimal Bonds6MonthMom { get; set; }
        public decimal Bonds12MonthMom { get; set; }
        public decimal BondsAverageMomentum { get; set; }

        public StrategyResult Result { get; set; }
    }
}
