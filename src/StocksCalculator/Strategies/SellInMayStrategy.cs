using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StocksCalculator.Models;

namespace StocksCalculator.Strategies
{
    public class SellInMayStrategy
    {
        public SellInMayResult Compute(List<StockPrice> prices, DateTime dateTime)
        {
            var trendFollow = new TrendFollowingStrategy();
            var tffResult = trendFollow.Compute(prices, dateTime.AddMonths(-1));
            var tffResultCurrent = trendFollow.Compute(prices, dateTime);

            if (tffResult.Result == StrategyResult.None)
                return new SellInMayResult { Result = StrategyResult.None };

            var result = new SellInMayResult
            {
                Result = tffResult.Result == StrategyResult.Stocks
                         && (dateTime.Month >= 10 || dateTime.Month < 5)
                    ? StrategyResult.Stocks
                    : StrategyResult.Bonds,
                Average = tffResultCurrent.Average
            };

            return result;
        }
    }
}
