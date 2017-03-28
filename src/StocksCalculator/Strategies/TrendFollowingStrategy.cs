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
    public class TrendFollowingStrategy : IStrategy
    {
        public List<TrendFollowingResult> TrendFollowingResult { get; } = new List<TrendFollowingResult>();
        public List<IStrategyResult> Results => TrendFollowingResult.Select(r => (IStrategyResult)r).ToList();
        private const int Months = 12;

        public void Compute(List<StockPrice> prices, DateTime dateTime)
        {
            TrendFollowingResult.Add(ComputeSingle(prices, dateTime));
        }

        public TrendFollowingResult ComputeSingle(List<StockPrice> prices, DateTime dateTime)
        {
            var result = new TrendFollowingResult()
            {
                Date = dateTime
            };

            var yearData = prices.Where(p => p.Date > dateTime.AddMonths(-Months) && p.Date <= dateTime).ToList();
            if (yearData.Count < Months)
            {
                return result;
            }

            if (yearData.Count > Months)
            {
                throw new InvalidOperationException($"Year data count should be no more then {Months} months");
            }

            result.Average = yearData.Average(p => p.Snp500);
            result.Result = prices
                .Single(p => p.Date.CompareByMonth(dateTime)).Snp500 > result.Average
                ? StrategyResult.Stocks : StrategyResult.Bonds;

            return result;
        }
    }
}
