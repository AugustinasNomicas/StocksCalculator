using System;
using StocksCalculator.Extensions;
using StocksCalculator.Models.PortfolioCalculator;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using StocksCalculator.Models;

namespace StocksCalculator
{
    public class Program
    {
        public const string Snp500Ticker = "^GSPC";
        public const string BondsTicker = "VUSTX";

        public const string Snp500MultiplierTicker = "SSO";
        public const string BondsMultiplierTicker = "UBT";

        public static void Main(string[] args)
        {
            Console.WriteLine("Welcome to intelligent stocks calculator");

            var portfolio = new PortfolioCalculator();
            var performance = new PerformanceCalculator(); ;

            var result = portfolio.Calculate(new CalculateRequest
            {
                StocksTicker = Snp500Ticker,
                BondsTicker = BondsTicker,
                StartDate = new DateTime(2014, 1, 1),
                EndDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
            });

            var results = performance.Calculate(result, 5000, 10, true, Snp500MultiplierTicker, BondsMultiplierTicker);
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

            IList<StrategyResult> results = new List<StrategyResult>();
            foreach (var month in monthlyResults)
            {
                results = portfolio.Strategies.Select(s => s.Results.Single(r => r.Date == month.Date).Result).ToList();
                ConsoleTable.PrintRow(month.Date.ToString("MMM-yy"), results[0], results[1], results[2], results[3], results[4]);
            }

            Console.WriteLine();
            Console.WriteLine("Results:");
            Console.WriteLine($"Stocks: {results.Count(r => r == StrategyResult.Stocks) * 20}%");
            Console.WriteLine($"Bonds: {results.Count(r => r == StrategyResult.Bonds) * 20}%");
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

