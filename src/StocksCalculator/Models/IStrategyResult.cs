using System;
using System.Collections.Generic;

namespace StocksCalculator.Models
{
    public interface IStrategy
    {
        List<IStrategyResult> Results { get; }
        void Compute(List<StockPrice> prices, DateTime dateTime);
    }
    public interface IStrategyResult
    {
        DateTime Date { get; set; }
        StrategyResult Result { get; set; }
    }
}
