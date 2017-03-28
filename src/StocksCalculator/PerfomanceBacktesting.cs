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

        public void BackTest(string snp500Ticker, string bondsTicker)
        {
            var sYear = DateTime.Now.AddYears((YearsToBackTest + 10) * -1).Year;
            var eYear = DateTime.Now.Year;

            var stockPrices = yahooFinanceService.GetStockPrices(sYear, eYear, snp500Ticker, bondsTicker);

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


    }
}
