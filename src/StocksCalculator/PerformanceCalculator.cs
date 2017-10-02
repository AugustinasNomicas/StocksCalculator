using System;
using StocksCalculator.Extensions;
using StocksCalculator.Models;
using StocksCalculator.Models.PortfolioCalculator;
using StocksCalculator.Services;
using System.Collections.Generic;
using System.Linq;

namespace StocksCalculator
{
    public class PerformanceCalculator
    {
        public PerformanceResults Calculate(List<CalculateMonthResult> monthlyResults, decimal StartMoneyAmount,
            decimal TransactionFee, bool useMultiplier, string stocksMultiplierTicker, string bondsMultiplierTicker)
        {
            var result = new PerformanceResults();
            var valueInCash = StartMoneyAmount;
            decimal cash = 0;

            CalculateMonthResult lastMonth = null;

            List<StockPrice> stockMultiplierPrices = null;
            if (useMultiplier)
            {
                var stockPrices = new StocksCsvFileReader();
                stockMultiplierPrices = stockPrices.GetStockPrices(monthlyResults.First().Date.Year,
                    monthlyResults.Last().Date.Year,
                    stocksMultiplierTicker, bondsMultiplierTicker, monthlyResults.Last().Date.CompareByMonth(DateTime.Now) && DateTime.Now.Day == 1);
            }

            foreach (var month in monthlyResults)
            {
                if (useMultiplier)
                {
                    month.StockPrice = stockMultiplierPrices.Single(d => d.Date == month.Date).Snp500;
                    month.BondPrice = stockMultiplierPrices.Single(d => d.Date == month.Date).Bonds;
                }

                if (lastMonth != null)
                {
                    valueInCash = month.StockPrice * lastMonth.StocksInPortfolio
                            + month.BondPrice * lastMonth.BondsInPortfolio + cash;
                }

                var moneyForStocks = valueInCash * month.StocksRatio;
                var moneyForBonds = valueInCash * month.BondsRatio;

                month.StocksInPortfolio = (int)(moneyForStocks / month.StockPrice);
                month.BondsInPortfolio = (int)(moneyForBonds / month.BondPrice);

                cash = valueInCash - month.StocksInPortfolio * month.StockPrice - month.BondsInPortfolio * month.BondPrice;

                if (lastMonth == null || month.StocksRatio != lastMonth.StockPrice)
                {
                    month.TransactionFee = TransactionFee;
                }

                month.ValueInCash = valueInCash + cash;
                month.ValueInCashAfterFee = month.ValueInCash - month.TransactionFee;
                lastMonth = month;
            }

            result.MonthResults = monthlyResults;

            if (lastMonth != null)
            {
                result.TotalMoneyAmount = lastMonth.ValueInCash;
                result.TotalMoneyAmountAfterFee = lastMonth.ValueInCash - monthlyResults.Sum(m => m.TransactionFee);
            }

            return result;
        }
    }
}
