using System;
using StocksCalculator.Extensions;
using StocksCalculator.Models.PortfolioCalculator;
using System.Collections.Generic;
using System.Linq;
using StocksCalculator.Strategies;

namespace StocksCalculator
{
    public class Program
    {
        public const string Snp500Ticker = "^GSPC"; // ^GSPC"; // SSO
        public const string BondsTicker = "VUSTX"; //VUSTX"; //UBT
        private const string DateFormat = "MMM-yy";
        public static void Main(string[] args)
        {
            Console.WriteLine("Welcome to intelligent stocks calculator");

            var portfolio = new PortfolioCalculator();
            var performance = new PerformanceCalculator();

            var result = portfolio.Calculate(new CalculateRequest
            {
                StocksTicker = Snp500Ticker,
                BondsTicker = BondsTicker,
                StartDate = new DateTime(2007, 1, 1),
                EndDate = new DateTime(2017, 5, 1),
            });

            var results = performance.Calculate(result, 5000, 10);
            PrintMontlyResults(results.MonthResults);
            ConsoleTable.PrintLine();
            ConsoleTable.PrintLine();
            PrintYearlyResults(results.MonthResults);

            Console.WriteLine("Total total cash value: {0}", results.TotalMoneyAmount);
            Console.WriteLine("Total total cash value after transaction fees: {0}", results.TotalMoneyAmountAfterFee);
            Console.WriteLine();

            PrintStrategyResults(results.MonthResults, portfolio);
            Console.WriteLine();

            Console.WriteLine("Done. Thanks. Go away.");
            Console.ReadKey();
        }

        private static void PrintStrategyResults(List<CalculateMonthResult> monthlyResults, PortfolioCalculator portfolio)
        {
            Console.WriteLine("Startegy results:");
            ConsoleTable.PrintRow("Date", "TD", "Momentum", "SellInMay", "ECRI", "OECD");

            foreach (var month in monthlyResults)
            {
                var results = portfolio.Strategies.Select(s => s.Results.Single(r => r.Date == month.Date).Result).ToList();
                ConsoleTable.PrintRow(month.Date.ToString("MMM-yy"), results[0], results[1], results[2], results[3], results[4]);
            }
        }

        private static void PrintYearlyResults(List<CalculateMonthResult> monthlyResults)
        {
            ConsoleTable.PrintRow("Date", "ValueInCash", "Value Change", "Percentage");
            var yearFirstMonths = monthlyResults.Where(m => m.Date.Month == 1).ToList();
            var previuosYear = yearFirstMonths.First();

            var yearRows = new List<(decimal value, decimal change, decimal percentage)>();
            var yearRowsAfterFee = new List<(decimal value, decimal change, decimal percentage)>();
            foreach (var year in yearFirstMonths)
            {
                yearRows.Add((year.ValueInCash,
                    year.ValueInCash - previuosYear.ValueInCash,
                    (year.ValueInCash / previuosYear.ValueInCash - 1)));

                yearRowsAfterFee.Add((year.ValueInCashAfterFee,
                    year.ValueInCashAfterFee - previuosYear.ValueInCashAfterFee,
                    (year.ValueInCashAfterFee / previuosYear.ValueInCashAfterFee - 1)));


                ConsoleTable.PrintRow(year.Date.ToString("MMM-yy"),
                    yearRowsAfterFee.Last().value,
                    yearRowsAfterFee.Last().change,
                    yearRowsAfterFee.Last().percentage.ToString("P"));

                previuosYear = year;
            }
            ConsoleTable.PrintLine();

            Console.WriteLine();
            Console.WriteLine("Number of years tested: {0}", yearRows.Count);
            Console.WriteLine("Average return rate: {0:P}", yearRows.Skip(1).Average(y => y.percentage));
            //Console.WriteLine("Average return rate: {0:P} (after transaction fee)", yearRowsAfterFee.Average(y => y.percentage));
            Console.WriteLine();
        }

        private static void PrintMontlyResults(List<CalculateMonthResult> monthlyResults)
        {
            ConsoleTable.PrintRow("Month", "StockPrice", "BondPrice", "StockRation", "Stocks", "Bonds", "Value", "Cash");
            monthlyResults.Where(m => m.Date.Month == 1).ToList().ForEach(month =>
            ConsoleTable.PrintRow(month.Date.ToString("MMM-yy"),
                month.StockPrice,
                month.BondPrice,
                month.StocksRatio.ToString("P"),
                month.StocksInPortfolio,
                month.BondsInPortfolio,
                month.ValueInCash
            ));
        }
    }
}

