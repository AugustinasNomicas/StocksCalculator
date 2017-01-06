using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StocksCalculator.Extensions;
using StocksCalculator.Models;

namespace StocksCalculator.Strategies
{
    // Strategy target is to compute moving average of 12 month and compare to current value
    // Returns: 
    // average and bool result
    // true = buy stocks, false = sell stocks
    public class TrendFollowing
    {
        public Tuple<double, bool> Compute(List<StockPrice> historicalPrices, DateTime dateTime)
        {
            var yearData = historicalPrices.Where(p => p.Date > dateTime.AddMonths(-12) && p.Date <= dateTime).ToList();
            if (yearData.Count < 12)
            {
                return new Tuple<double, bool>(0, false);
            }

            if (yearData.Count > 12)
            {
                throw new InvalidOperationException("Year data count should be no more then 12 months");
            }

            var average = yearData.Average(p => p.Snp500);
            var shouldBuy = historicalPrices.Single(p => p.Date.CompareByMonth(dateTime)).Snp500 > average;
            return new Tuple<double, bool>(average, shouldBuy);
        }
    }
}
