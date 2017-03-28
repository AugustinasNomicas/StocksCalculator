using StocksCalculator.Extensions;
using StocksCalculator.Models;
using StocksCalculator.Services;
using StocksCalculator.Strategies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StocksCalculator
{
    public class PerfomanceBackTesting
    {
        public const string Snp500Ticker = "^GSPC";
        public const string BondsTicker = "VUSTX";
        public const int YearsToBackTest = 20;
        YahooFinanceService yahooFinanceService;
        private const string DateFormat = "MMM-yy";

        public List<IStrategy> Strategies = new List<IStrategy>
        {
            new TrendFollowingStrategy(),
            new MomentumStrategy(),
            new SellInMayStrategy(),
            new EcriStrategy(),
            new OecdStrategy()
        };

        public PerfomanceBackTesting()
        {
            yahooFinanceService = new YahooFinanceService();
        }

        public void BackTest()
        {
            var sYear = DateTime.Now.AddYears((YearsToBackTest + 10) * -1).Year;
            var eYear = DateTime.Now.Year;

            var stockPrices = GetStockPrices(sYear, eYear);

            ConsoleTable.PrintRow("Date", "TF", "Momentum", "SellInMay", "Ecri", "Oecd");

            foreach (var stockPrice in stockPrices)
            {
                foreach (var strategy in Strategies)
                {
                    strategy.Compute(stockPrices, stockPrice.Date);
                }

                ConsoleTable.PrintRow(stockPrice.Date.ToString(DateFormat)
                    , Strategies[0].Results.LastOrDefault()?.Result
                    , Strategies[1].Results.LastOrDefault()?.Result
                    , Strategies[2].Results.LastOrDefault()?.Result
                    , Strategies[3].Results.LastOrDefault()?.Result
                    , Strategies[4].Results.LastOrDefault()?.Result);
            }
        }

        private List<StockPrice> GetStockPrices(int sYear, int eYear)
        {
            Console.WriteLine($"Getting data from yahoo from {sYear} to {eYear}");
            List<YahooHistoricalStock> snp500 = null;
            List<YahooHistoricalStock> bonds = null;

            Task.Run(async () =>
            {
                snp500 = await yahooFinanceService.DownloadDataAsync(Snp500Ticker, sYear, eYear);
                bonds = await yahooFinanceService.DownloadDataAsync(BondsTicker, sYear, eYear);
            }).Wait();

            return snp500.Select(s => new StockPrice
            {
                Date = new DateTime(s.Date.Year, s.Date.Month, 1),
                Snp500 = s.AdjClose,
                Bonds = bonds.Single(b => b.Date == s.Date).AdjClose
            }).OrderBy(s => s.Date).ToList();
        }
    }
}
