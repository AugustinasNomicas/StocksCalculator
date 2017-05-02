using StocksCalculator.Extensions;
using StocksCalculator.Models.PortfolioCalculator;
using System.Collections.Generic;
using System.Linq;

namespace StocksCalculator
{
    public class PerformanceCalculator
    {
        public PerformanceResults Calculate(List<CalculateMonthResult> monthlyResults, decimal StartMoneyAmount, decimal TransactionFee)
        {
            var result = new PerformanceResults();
            var valueInCash = StartMoneyAmount;
            decimal cash = 0;

            CalculateMonthResult lastMonth = null;

            foreach (var month in monthlyResults)
            {
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
