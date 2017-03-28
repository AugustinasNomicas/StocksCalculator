using System;
using System.Collections.Generic;
using System.Linq;
using StocksCalculator.Models;

namespace StocksCalculator.Strategies
{
    public class MomentumStrategy : IStrategy
    {
        public List<MomentumResult> MomentumResult { get; } = new List<MomentumResult>();
        public List<IStrategyResult> Results => MomentumResult.Select(r => (IStrategyResult)r).ToList();

        public void Compute(List<StockPrice> prices, DateTime dateTime)
        {
            var result = new MomentumResult
            {
                Date = dateTime
            };

            var trendFollowing = new TrendFollowingStrategy();
            var tffFilter = trendFollowing.ComputeSingle(prices, dateTime);

            if (tffFilter.Result == StrategyResult.None
                || prices.SingleOrDefault(p => p.Date == dateTime.AddMonths(-12)) == null)
            {
                MomentumResult.Add(result);
                return;
            }

            var currentPrice = prices.Single(p => p.Date == dateTime);

            result.Stocks12MonthMovingAverage = tffFilter.Average;
            result.Stocks3MonthMom = currentPrice.Snp500 / prices.Single(p => p.Date == dateTime.AddMonths(-3)).Snp500 - 1;
            result.Stocks6MonthMom = currentPrice.Snp500 / prices.Single(p => p.Date == dateTime.AddMonths(-6)).Snp500 - 1;
            result.Stocks12MonthMom = currentPrice.Snp500 / prices.Single(p => p.Date == dateTime.AddMonths(-12)).Snp500 - 1;
            result.StocksAverageMomentum = (result.Stocks3MonthMom + result.Stocks6MonthMom + result.Stocks12MonthMom) / 3;

            result.TffFilter = tffFilter.Result == StrategyResult.Stocks;

            result.Bonds3MonthMom = currentPrice.Bonds / prices.Single(p => p.Date == dateTime.AddMonths(-3)).Bonds - 1;
            result.Bonds6MonthMom = currentPrice.Bonds / prices.Single(p => p.Date == dateTime.AddMonths(-6)).Bonds - 1;
            result.Bonds12MonthMom = currentPrice.Bonds / prices.Single(p => p.Date == dateTime.AddMonths(-12)).Bonds - 1;
            result.BondsAverageMomentum = (result.Bonds3MonthMom + result.Bonds6MonthMom + result.Bonds12MonthMom) / 3;

            result.Result = (result.StocksAverageMomentum >
                result.BondsAverageMomentum && result.TffFilter)
                ? StrategyResult.Stocks
                : StrategyResult.Bonds;

            MomentumResult.Add(result);
        }

    }
}
