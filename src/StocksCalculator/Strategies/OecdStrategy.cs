using StocksCalculator.Extensions;
using StocksCalculator.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace StocksCalculator.Strategies
{
    public class OecdStrategy : IStrategy
    {
        private const int Months = 12;
        private const string OECDCsv = @"..\..\OECD.csv";
        private bool TestingCsvFormat = false; // indicates we are using simple CSV format for data copied from Mokymai spreadsheets
        private List<(DateTime date, decimal level)> CsvData;

        private List<OecdResult> OecdResults = new List<OecdResult>();
        public List<IStrategyResult> Results => OecdResults.Select(r => (IStrategyResult)r).ToList();

        public void PrintDetails(List<StockPrice> prices)
        {
            if (OecdResults.Any())
            {
                throw new InvalidOperationException("OecdResults must be empty");
            }

            Console.WriteLine("OECD result:");
            ConsoleTable.PrintRow("Date", "Bonds", "Cycle", "Aver1", "Aver2", "Aver3", "Aver4");

            foreach (var price in prices)
            {
                Compute(prices, price.Date);
                var result = OecdResults.Single(r => r.Date == price.Date);
                if (result.Result != StrategyResult.None)
                {
                    ConsoleTable.PrintRow(
                        result.Date.ToString("MMM-yy"),
                        price.Bonds,
                        result.CyclePhase,
                        result.BondsAvgReturnByCycle[0].AvgReturn.ToString("P"),
                        result.BondsAvgReturnByCycle[1].AvgReturn.ToString("P"),
                        result.BondsAvgReturnByCycle[2].AvgReturn.ToString("P"),
                        result.BondsAvgReturnByCycle[3].AvgReturn.ToString("P"),
                        result.Result);
                }

            }

            ConsoleTable.PrintLine();
        }

        public void Compute(List<StockPrice> prices, DateTime dateTime)
        {
            var result = new OecdResult
            {
                Date = dateTime,
            };
            OecdResults.Add(result);

            if (CsvData == null)
            {
                CsvData = ReadDataFromCsv().ToList();
            }

            if (!FillLevel(result))
            {
                return;
            };

            if (!ComputeLevelChangeAndCyclePhase(result) || !result.CyclePhaseTwoMonthsOld.HasValue)
            {
                return;
            }

            var stocksComputed = ComputeStocks(prices, result);
            var bondsComputed = ComputeBonds(prices, dateTime, result);

            if (!stocksComputed || !bondsComputed)
                return;

            var lastMonth = OecdResults.Single(e => e.Date == dateTime.AddMonths(-1));

            if (!lastMonth.StocksAvgReturnByCycle.Any())
            {
                return;
            }

            for (var i = 0; i < 4; i++)
            {
                result.StocksAvgReturnByCycle[i].Result = (lastMonth.StocksAvgReturnByCycle[i].AvgReturn >
                                                           lastMonth.BondsAvgReturnByCycle[i].AvgReturn
                                                           && lastMonth.StocksTrendFollowFilter);
                result.BondsAvgReturnByCycle[i].Result = !result.StocksAvgReturnByCycle[i].Result;
            }

            if (result.CyclePhaseTwoMonthsOld.HasValue)
            {
                result.Result = result.StocksAvgReturnByCycle[result.CyclePhaseTwoMonthsOld.Value - 1].Result
                    ? StrategyResult.Stocks
                    : StrategyResult.Bonds;
            }


        }

        private bool FillLevel(OecdResult result)
        {
            var csvRow = CsvData.SingleOrDefault(c => c.date == result.Date);
            if (csvRow.date == default(DateTime))
            {
                return false;
            }

            result.OecdLevel = csvRow.level;
            return true;
        }

        private bool ComputeLevelChangeAndCyclePhase(OecdResult result)
        {
            if (!OecdResults.Any())
                return false;

            var lastOecdLevel = OecdResults.TakeLast(2).First().OecdLevel;
            var currentOecdLevel = result.OecdLevel;
            result.OecdLevelChange = currentOecdLevel - lastOecdLevel;

            result.CyclePhase = 4;

            if (currentOecdLevel > 100 && result.OecdLevelChange < 0)
            {
                result.CyclePhase = 1;
            }

            if (currentOecdLevel < 100 && result.OecdLevelChange < 0)
            {
                result.CyclePhase = 2;
            }

            if (currentOecdLevel < 100 && result.OecdLevelChange > 0)
            {
                result.CyclePhase = 3;
            }

            result.CyclePhaseTwoMonthsOld = OecdResults.SingleOrDefault(e => e.Date == result.Date.AddMonths(-2))
                ?.CyclePhase;

            return true;
        }

        private bool ComputeStocks(List<StockPrice> prices, OecdResult result)
        {
            var dateTime = result.Date;
            result.StocksReturn = prices.Single(p => p.Date == dateTime).Snp500
                / prices.Single(p => p.Date == dateTime.AddMonths(-1)).Snp500 - 1;

            result.StocksMovingAvg =
                prices.Where(p => p.Date > dateTime.AddMonths(-Months) && p.Date <= dateTime)
                .Select(s => s.Snp500)
                .Average();

            var months = 120;
            if (!GetOecd10YearData(dateTime, months, out List<OecdResult> oecd10YearData)) return false;

            result.StocksAvgReturnByCycle.Clear();
            for (byte cycle = 1; cycle <= 4; cycle++)
            {
                var filtered = oecd10YearData.Where(e => e.CyclePhaseTwoMonthsOld == cycle).ToList();
                var returnByCycle = filtered.Any() ? filtered.Average(r => r.StocksReturn) : -1;

                result.StocksAvgReturnByCycle.Add(new AvgReturnByCycle { AvgReturn = returnByCycle, CyclePhase = cycle });
            }

            result.StocksTrendFollowFilter = prices.Single(p => p.Date == dateTime).Snp500 > result.StocksMovingAvg;

            return true;
        }

        private bool ComputeBonds(List<StockPrice> prices, DateTime dateTime, OecdResult result)
        {
            result.BondsReturn = prices.Single(p => p.Date == dateTime).Bonds
                / prices.Single(p => p.Date == dateTime.AddMonths(-1)).Bonds - 1;

            var months = 120;
            if (!GetOecd10YearData(dateTime, months, out List<OecdResult> Oecd10YearData)) return false;

            result.BondsAvgReturnByCycle.Clear();
            for (byte cycle = 1; cycle <= 4; cycle++)
            {
                var filtered = Oecd10YearData.Where(e => e.CyclePhaseTwoMonthsOld == cycle).ToList();
                var returnByCycle = filtered.Any() ? filtered.Average(r => r.BondsReturn) : -99999;
                result.BondsAvgReturnByCycle.Add(new AvgReturnByCycle { AvgReturn = returnByCycle, CyclePhase = cycle });
            }

            return true;
        }

        private bool GetOecd10YearData(DateTime dateTime, int months, out List<OecdResult> oecd10YearData)
        {
            // inclusive:
            // from minus one year and one month
            // to dateTime

            oecd10YearData = OecdResults.Where(p =>
                    p.Date >= dateTime.AddMonths(-months).AddMonths(-1)
                    && p.Date <= dateTime).ToList();

            if (oecd10YearData.Count < months)
            {
                return false;
            }

            if (oecd10YearData.Any(e => e.CyclePhaseTwoMonthsOld == null))
            {
                return false;
            }

            var realMonthsCount = months + 2;
            if (oecd10YearData.Count > realMonthsCount)
            {
                throw new InvalidOperationException($"10 Year data count should be no more then {realMonthsCount} months");
            }
            return true;
        }


        private IEnumerable<(DateTime date, decimal level)> ReadDataFromCsv()
        {
            var rows = File.ReadLines(OECDCsv);
            //First row is headers so skip it
            foreach (var row in rows.Skip(1))
            {
                DateTime date;
                decimal level;

                if (row.Replace("\n", "").Trim() == "") continue;
                var cols = row.Split(',');

                if (TestingCsvFormat)
                {
                    date = DateTime.ParseExact(cols[0], "MMM-yy", CultureInfo.InvariantCulture);
                    level = Convert.ToDecimal(cols[1]);
                }
                else
                {
                    var dateAsStr = cols[7].Replace("\"", "");
                    date = DateTime.ParseExact(dateAsStr, "MMM-yyyy", CultureInfo.InvariantCulture);
                    level = Convert.ToDecimal(cols[14]);
                }

                yield return (date, level);
            }
        }
    }
}
