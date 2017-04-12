using System;
using System.Collections.Generic;
using System.Linq;
using StocksCalculator.Models;

namespace StocksCalculator.Strategies
{
    public class SellInMayStrategy : IStrategy
    {
        private List<SellInMayResult> SellInMayResults = new List<SellInMayResult>();
        public List<IStrategyResult> Results => SellInMayResults.Select(r => (IStrategyResult)r).ToList();

        public void Compute(List<StockPrice> prices, DateTime dateTime)
        {
            var trendFollow = new TrendFollowingStrategy();
            var tffResult = trendFollow.ComputeSingle(prices, dateTime.AddMonths(-1));
            var tffResultCurrent = trendFollow.ComputeSingle(prices, dateTime);

            if (tffResult.Result == StrategyResult.None)
            {
                SellInMayResults.Add(new SellInMayResult { Result = StrategyResult.None, Date = dateTime });
                return;
            }

            var result = new SellInMayResult
            {
                Date = dateTime,
                Result = tffResult.Result == StrategyResult.Stocks
                         && (dateTime.Month >= 10 || dateTime.Month < 5)
                    ? StrategyResult.Stocks
                    : StrategyResult.Bonds,
                Average = tffResultCurrent.Average
            };

            SellInMayResults.Add(result);
            return;

        }
    }
}
