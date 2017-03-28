using System;

namespace StocksCalculator.Models
{
    public class TrendFollowingResult : IStrategyResult
    {
        public DateTime Date { get; set; }
        public decimal Average { get; set; }
        public StrategyResult Result { get; set; }
    }
}
