using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StocksCalculator.Models;
using StocksCalculator.Services;
using StocksCalculator.Strategies;

namespace StocksCalculator
{
    public class Program
    {
        public const string Snp500Ticker = "^GSPC";
        public const string BondsTicker = "VUSTX";
        public const int YearsToBackTest = 3;

        public static void Main(string[] args)
        {
            Console.WriteLine("Welcome to intelligent stocks calculator");

            var sYear = DateTime.Now.AddYears((YearsToBackTest + 1) * -1).Year;
            var eYear = DateTime.Now.Year;

            var fin = new YahooFinanceService();
            var trendFollowing = new TrendFollowing();

            Console.WriteLine($"Getting data from yahoo from {sYear} to {eYear}");

            List<YahooHistoricalStock> snp500 = null;
            List<YahooHistoricalStock> bonds = null;

            Task.Run(async () =>
            {
                snp500 = await fin.DownloadDataAsync(Snp500Ticker, sYear, eYear);
                bonds = await fin.DownloadDataAsync(BondsTicker, sYear, eYear);
            }).Wait();

            var stockPrices = snp500.Select(s => new StockPrice
            {
                Date = new DateTime(s.Date.Year, s.Date.Month, 1),
                Snp500 = s.AdjClose,
                Bonds = bonds.Single(b => b.Date == s.Date).AdjClose
            }
            ).OrderBy(s => s.Date).ToList();

            Console.WriteLine("Stock prices:");
            stockPrices.ForEach(r => Console.WriteLine($"{r.Date} S&P500: {r.Snp500} Bonds: {r.Bonds}"));
            Console.WriteLine("");
            Console.WriteLine("--------------------------------");
            Console.WriteLine("Trend following result:");

            stockPrices.ForEach(r =>
            {
                var result = trendFollowing.Compute(stockPrices, r.Date.AddMonths(-1));
                if (result.Item1 > 0)
                {
                    Console.WriteLine(
                        $"{r.Date} S&P500: {r.Snp500} Bonds: {r.Bonds} Average: {result.Item1} Result: {result.Item2}");
                }
            });

            Console.WriteLine("Done. Thanks. Go away.");
            Console.ReadKey();

        }
    }
}
