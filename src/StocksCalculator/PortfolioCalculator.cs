using StocksCalculator.Extensions;
using StocksCalculator.Models;
using StocksCalculator.Models.PortfolioCalculator;
using StocksCalculator.Services;
using StocksCalculator.Strategies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StocksCalculator
{
    public class PortfolioCalculator
    {
        YahooFinanceService yahooFinanceService;

        public List<IStrategy> Strategies { get; set; }

        public PortfolioCalculator()
        {
            yahooFinanceService = new YahooFinanceService();
        }

        private void InitStrategies()
        {
            Strategies = new List<IStrategy>
            {
                new TrendFollowingStrategy(),
                new MomentumStrategy(),
                new SellInMayStrategy(),
                new EcriStrategy(),
                new OecdStrategy()
            };
        }

        public List<CalculateMonthResult> Calculate(CalculateRequest request)
        {
            InitStrategies();
            var sYear = request.StartDate.AddYears(-10);
            var eYear = request.EndDate;
            var result = new List<CalculateMonthResult>();

            var stockPrices = yahooFinanceService.GetStockPrices(sYear.Year, eYear.Year, request.StocksTicker,
                request.BondsTicker);

            foreach (var stockPrice in stockPrices)
            {
                foreach (var strategy in Strategies)
                {
                    strategy.Compute(stockPrices, stockPrice.Date);
                }

                var ratio = CalculateStockAndRatio(stockPrice.Date, Strategies);
                if (ratio.validResult)
                {
                    result.Add(new CalculateMonthResult
                    {
                        Date = stockPrice.Date,
                        StocksRatio = ratio.stocks,
                        BondsRatio = ratio.bonds,
                        StockPrice = stockPrice.Snp500,
                        BondPrice = stockPrice.Bonds
                    });
                }
            }

            return result;
        }

        private (decimal stocks, decimal bonds, bool validResult) CalculateStockAndRatio(DateTime date, List<IStrategy> strategies)
        {
            var strategiesInStock = strategies.Count(r => r.Results.SingleOrDefault(r2 => r2.Date == date)?.Result == StrategyResult.Stocks);
            var strategiesInBonds = strategies.Count(r => r.Results.SingleOrDefault(r2 => r2.Date == date)?.Result == StrategyResult.Bonds);

            if (strategiesInStock + strategiesInBonds != strategies.Count)
            {
                return (0, 0, false);
            }

            decimal stocksRatio = strategiesInStock > 0 ? (decimal)strategiesInStock / strategies.Count : 0;
            decimal bondsRation = strategiesInBonds > 0 ? (decimal)strategiesInBonds / strategies.Count : 0;
            return (stocksRatio, bondsRation, true);
        }

    }
}
