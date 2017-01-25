using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using StocksCalculator.Extensions;
using StocksCalculator.Models;
using StocksCalculator.Services;
using StocksCalculator.Strategies;

namespace StocksCalculator
{
    public class Program
    {
        public const string Snp500Ticker = "^GSPC";
        public const string BondsTicker = "VUSTX";
        public const int YearsToBackTest = 20;
        private const string DateFormat = "MMM-yy";
        public static void Main(string[] args)
        {
            Console.WriteLine("Welcome to intelligent stocks calculator");

            var sYear = DateTime.Now.AddYears((YearsToBackTest + 10) * -1).Year;
            var eYear = 2014; // DateTime.Now.Year;

            var fin = new YahooFinanceService();
            var trendFollowing = new TrendFollowingStrategy();
            var momentum = new MomentumStrategy();
            var sellInMay = new SellInMayStrategy();
            var ecri = new EcriStrategy();

            Console.WriteLine($"Getting data from yahoo from {sYear} to {eYear}");

            List<YahooHistoricalStock> snp500 = null;
            List<YahooHistoricalStock> bonds = null;

            Task.Run(async () =>
            {
                snp500 = await fin.DownloadDataAsync(Snp500Ticker, sYear, eYear);
                bonds = await fin.DownloadDataAsync(BondsTicker, sYear, eYear);
                //bonds = fin.ReadDataFromFile(@"C:\Users\nomicaug\Dropbox\Investavimas\Mokymai\Data\Vanguard Long-Term Treasury Inv (VUSTX).csv");
            }).Wait();

            var stockPrices = snp500.Select(s => new StockPrice
            {
                Date = new DateTime(s.Date.Year, s.Date.Month, 1),
                Snp500 = s.AdjClose,
                Bonds = bonds.Single(b => b.Date == s.Date).AdjClose
            }
            ).OrderBy(s => s.Date).ToList();

            Console.WriteLine("Stock prices:");
            ConsoleTable.PrintRow("Date", "S&P500", "Bonds");
            ConsoleTable.PrintLine();
            stockPrices.ForEach(r => ConsoleTable.PrintRow(r.Date.ToString(DateFormat), r.Snp500, r.Bonds));
            ConsoleTable.PrintLine();

            //TrendFollowing(stockPrices, trendFollowing);
            //Momentum(stockPrices, momentum);
            //SellInMay(stockPrices, sellInMay);
            Ecri(stockPrices, ecri);


            Console.WriteLine("Done. Thanks. Go away.");
            Console.ReadKey();

        }

        private static void Momentum(List<StockPrice> stockPrices, MomentumStrategy strategy)
        {
            ConsoleTable.PrintLine();
            Console.WriteLine("Momentum result:");

            ConsoleTable.PrintLine();
            ConsoleTable.PrintRow("Date", "S&P500", "12mMA", "3Mom", "6Mom", "12Mom", "AvMom", "TFF", "VANG",
                "3Mom", "6Mom", "12Mom", "AvMom", "Result");

            MomentumComputations momentumComputations = null;
            StrategyResult result = StrategyResult.None;

            stockPrices.ForEach(r =>
            {
                if (momentumComputations != null && momentumComputations.CanComputeResult)
                {
                    result = strategy.ComputeResult(momentumComputations);
                }

                momentumComputations = strategy.Compute(stockPrices, r.Date);
                ConsoleTable.PrintRow(r.Date.ToString(DateFormat),
                    r.Snp500,
                    momentumComputations.Stocks12MonthMovingAverage,
                    momentumComputations.Stocks3MonthMom.ToString("P"),
                    momentumComputations.Stocks6MonthMom.ToString("P"),
                    momentumComputations.Stocks12MonthMom.ToString("P"),
                    momentumComputations.StocksAverageMomentum.ToString("P"),
                    momentumComputations.TffFilter,
                    r.Bonds,
                    momentumComputations.Bonds3MonthMom.ToString("P"),
                    momentumComputations.Bonds6MonthMom.ToString("P"),
                    momentumComputations.Bonds12MonthMom.ToString("P"),
                    momentumComputations.BondsAverageMomentum.ToString("P"),
                    result);
            });

            ConsoleTable.PrintLine();
        }

        private static void TrendFollowing(List<StockPrice> stockPrices, TrendFollowingStrategy strategy)
        {
            Console.WriteLine("Trend following result:");

            ConsoleTable.PrintLine();
            ConsoleTable.PrintRow("Date", "Average", "Result");
            stockPrices.ForEach(r =>
            {
                var result = strategy.Compute(stockPrices, r.Date.AddMonths(-1));
                if (result.Result != StrategyResult.None)
                {
                    ConsoleTable.PrintRow(r.Date.ToString(DateFormat), result.Average, result.Result);
                }
            });

            ConsoleTable.PrintLine();
        }

        private static void SellInMay(List<StockPrice> stockPrices, SellInMayStrategy strategy)
        {
            Console.WriteLine("Sell in may result:");

            ConsoleTable.PrintLine();
            ConsoleTable.PrintRow("Date", "Average", "Result");
            stockPrices.ForEach(r =>
            {
                var result = strategy.Compute(stockPrices, r.Date);
                if (result.Result != StrategyResult.None)
                {
                    ConsoleTable.PrintRow(r.Date.ToString(DateFormat), result.Average, result.Result);
                }
            });

            ConsoleTable.PrintLine();
        }

        private static void Ecri(List<StockPrice> stockPrices, EcriStrategy strategy)
        {
            Console.WriteLine("ECRI result:");

            ConsoleTable.PrintLine();
            //ConsoleTable.PrintRow("Date", "Level", "12mChng", "12MA", "12MA_Mom", "CyclePhase", "S&P500", "12ma", "StocksReturn", "Cycle",
            //    "10y1", "10y2", "10y3", "10y4", "TF");
            //stockPrices.ForEach(r =>
            //{
            //    strategy.Compute(stockPrices, r.Date);
            //    var result = strategy.EcriResults.Last();
            //    if (result.Result != StrategyResult.None)
            //    {
            //        ConsoleTable.PrintRow(r.Date.ToString(DateFormat),
            //          result.EcriLevel,
            //          result.EcriChange12M.ToString("P"),
            //          result.EcriMovingAvg12.ToString("P"),
            //          result.EcriMovingAvgMom12.ToString("P"),
            //          result.CyclePhase,
            //          r.Snp500,
            //          result.StocksMovingAvg,
            //          result.StocksReturn.ToString("P"),
            //          result.CyclePhaseTwoMonthsOld,
            //          result.StocksAvgReturnByCycle[0].AvgReturn.ToString("P"),
            //          result.StocksAvgReturnByCycle[1].AvgReturn.ToString("P"),
            //          result.StocksAvgReturnByCycle[2].AvgReturn.ToString("P"),
            //          result.StocksAvgReturnByCycle[3].AvgReturn.ToString("P"),
            //          result.StocksTrendFollowFilter);
            //    }
            //});

            ConsoleTable.PrintRow("Date", "Level", "12mChng", "12MA", "12MA_Mom", "CyclePhase", "VANG_LTT", "Return", "Cycle",
                "10y1", "10y2", "10y3", "10y4", "Result");
            stockPrices.ForEach(r =>
            {
                strategy.Compute(stockPrices, r.Date);
                var result = strategy.EcriResults.Last();
                if (result.Result != StrategyResult.None)
                {
                    ConsoleTable.PrintRow(r.Date.ToString(DateFormat),
                      result.EcriLevel,
                      result.EcriChange12M.ToString("P"),
                      result.EcriMovingAvg12.ToString("P"),
                      result.EcriMovingAvgMom12.ToString("P"),
                      result.CyclePhase,
                      r.Bonds,
                      result.BondsReturn.ToString("P"),
                      result.CyclePhaseTwoMonthsOld,
                      result.BondsAvgReturnByCycle[0].AvgReturn.ToString("P"),
                      result.BondsAvgReturnByCycle[1].AvgReturn.ToString("P"),
                      result.BondsAvgReturnByCycle[2].AvgReturn.ToString("P"),
                      result.BondsAvgReturnByCycle[3].AvgReturn.ToString("P"),
                      result.Result
                      );
                }
            });

            ConsoleTable.PrintLine();
        }
    }
}

