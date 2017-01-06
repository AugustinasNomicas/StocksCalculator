using System;
using System.Collections.Generic;
using System.Linq;
using StocksCalculator.Extensions;
using StocksCalculator.Models;

namespace StocksCalculator.Strategies
{
    // Strategy target is to compute moving average of 12 month and compare to current value
    // Returns: 
    // average and bool result
    // true = buy stocks, false = sell stocks
    public class TrendFollowingStrategy
    {
        private const int Months = 12;

        public TrendFollowingResult Compute(List<StockPrice> prices, DateTime dateTime)
        {
            var yearData = prices.Where(p => p.Date > dateTime.AddMonths(-Months) && p.Date <= dateTime).ToList();
            if (yearData.Count < Months)
            {
                return new TrendFollowingResult {Average = 0, Result = StrategyResult.None};
            }

            if (yearData.Count > Months)
            {
                throw new InvalidOperationException($"Year data count should be no more then {Months} months");
            }

            var average = yearData.Average(p => p.Snp500);
            var shouldBuy = prices.Single(p => p.Date.CompareByMonth(dateTime)).Snp500 > average;
            return new TrendFollowingResult {Average = average, Result = shouldBuy ? StrategyResult.Stocks : StrategyResult.Bonds};
        }
    }
}
