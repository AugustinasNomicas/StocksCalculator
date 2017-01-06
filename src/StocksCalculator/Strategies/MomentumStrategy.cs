using System;
using System.Collections.Generic;
using System.Linq;
using StocksCalculator.Models;

namespace StocksCalculator.Strategies
{
    public class MomentumStrategy
    {
        public MomentumComputations Compute(List<StockPrice> prices, DateTime dateTime)
        {
            var result = new MomentumComputations();

            var trendFollowing = new TrendFollowingStrategy();
            var tffFilter = trendFollowing.Compute(prices, dateTime);

            if (tffFilter.Result == StrategyResult.None
                || prices.SingleOrDefault(p => p.Date == dateTime.AddMonths(-12)) == null)
            {
                return result;
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

            result.CanComputeResult = true;

            return result;
        }

        public StrategyResult ComputeResult(MomentumComputations computations)
        {
            if (!computations.CanComputeResult)
                return StrategyResult.None;

            return (computations.StocksAverageMomentum > computations.BondsAverageMomentum && computations.TffFilter)
                ? StrategyResult.Stocks
                : StrategyResult.Bonds;
        }
    }
}
