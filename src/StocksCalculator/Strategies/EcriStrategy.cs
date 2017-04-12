using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using StocksCalculator.Extensions;
using StocksCalculator.Models;

namespace StocksCalculator.Strategies
{
    public class EcriStrategy : IStrategy
    {
        private const int Months = 12;
        private const string EcriCsv = @"..\..\ECRI.csv";

        private List<EcriResult> EcriResults = new List<EcriResult>();
        public List<IStrategyResult> Results => EcriResults.Select(r => (IStrategyResult)r).ToList();

        public void Compute(List<StockPrice> prices, DateTime dateTime)
        {
            var result = new EcriResult
            {
                Date = dateTime
            };

            // fill stock results with default values
            result.StocksAvgReturnByCycle.AddRange(
                Enumerable.Range(1, 4).Select(r => new AvgReturnByCycle
                { AvgReturn = 0, CyclePhase = (byte)r }));

            // fill bond results with default values
            result.BondsAvgReturnByCycle.AddRange(
                Enumerable.Range(1, 4).Select(r => new AvgReturnByCycle
                { AvgReturn = 0, CyclePhase = (byte)r }));

            EcriResults.Add(result);
            var cyclePhaseComputed = ComputeCyclePhase(result, dateTime);

            if (!cyclePhaseComputed)
                return;

            var stocksComputed = ComputeStocks(prices, dateTime, result);
            if (!stocksComputed)
                return;

            var bondsComputed = ComputeBonds(prices, dateTime, result);
            if (!bondsComputed)
                return;

            var lastMonth = EcriResults.Single(e => e.Date == dateTime.AddMonths(-1));
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

        private bool ComputeBonds(List<StockPrice> prices, DateTime dateTime, EcriResult result)
        {
            result.BondsReturn = prices.Single(p => p.Date == dateTime).Bonds
                / prices.Single(p => p.Date == dateTime.AddMonths(-1)).Bonds - 1;

            var months = 120;
            if (!GetEcri10YearData(dateTime, months, out List<EcriResult> ecri10YearData)) return false;

            result.BondsAvgReturnByCycle.Clear();
            for (byte cycle = 1; cycle <= 4; cycle++)
            {
                var filtered = ecri10YearData.Where(e => e.CyclePhaseTwoMonthsOld == cycle).ToList();
                var returnByCycle = filtered.Any() ? filtered.Average(r => r.BondsReturn) : -99999;
                result.BondsAvgReturnByCycle.Add(new AvgReturnByCycle { AvgReturn = returnByCycle, CyclePhase = cycle });
            }

            return true;
        }

        private bool ComputeStocks(List<StockPrice> prices, DateTime dateTime, EcriResult result)
        {
            result.StocksReturn = prices.Single(p => p.Date == dateTime).Snp500
                / prices.Single(p => p.Date == dateTime.AddMonths(-1)).Snp500 - 1;

            result.StocksMovingAvg =
                prices.Where(p => p.Date > dateTime.AddMonths(-Months) && p.Date <= dateTime)
                .Select(s => s.Snp500)
                .Average();

            var months = 120;
            if (!GetEcri10YearData(dateTime, months, out List<EcriResult> ecri10YearData)) return false;

            result.StocksAvgReturnByCycle.Clear();
            for (byte cycle = 1; cycle <= 4; cycle++)
            {
                var filtered = ecri10YearData.Where(e => e.CyclePhaseTwoMonthsOld == cycle).ToList();
                var returnByCycle = filtered.Any() ? filtered.Average(r => r.StocksReturn) : -1;

                result.StocksAvgReturnByCycle.Add(new AvgReturnByCycle { AvgReturn = returnByCycle, CyclePhase = cycle });
            }

            result.StocksTrendFollowFilter = prices.Single(p => p.Date == dateTime).Snp500 > result.StocksMovingAvg;

            return true;
        }

        private bool GetEcri10YearData(DateTime dateTime, int months, out List<EcriResult> ecri10YearData)
        {
            // inclusive:
            // from minus one year and one month
            // to dateTime

            ecri10YearData = EcriResults.Where(p =>
                    p.Date >= dateTime.AddMonths(-months).AddMonths(-1)
                    && p.Date <= dateTime).ToList();

            if (ecri10YearData.Count < months)
            {
                return false;
            }

            if (ecri10YearData.Any(e => e.CyclePhaseTwoMonthsOld == null))
            {
                return false;
            }

            var realMonthsCount = months + 2;
            if (ecri10YearData.Count > realMonthsCount)
            {
                throw new InvalidOperationException($"10 Year data count should be no more then {realMonthsCount} months");
            }
            return true;
        }

        private bool ComputeCyclePhase(EcriResult result, DateTime dateTime)
        {
            var ecriMonthlyData = ReadDataFromEcriCsv().GroupBy(w => new DateTime(w.date.Year, w.date.Month, 1))
                 .Select(m => m.Last()).Select(m => new Tuple<DateTime, decimal>(new DateTime(m.date.Year, m.date.Month, 1), m.level))
                 .ToList();

            var ecriYearData = ecriMonthlyData.Where(p => p.Item1 > dateTime.AddMonths(-Months) && p.Item1 <= dateTime).ToList();

            if (ecriYearData.Count < Months)
            {
                return false;
            }

            if (ecriYearData.Count > Months)
            {
                throw new InvalidOperationException($"Year data count should be no more then {Months} months");
            }

            result.EcriLevel = ecriYearData.Last().Item2;
            var ecriLevelBefore12Months = ecriMonthlyData.Single(e => e.Item1 == dateTime.AddMonths(-Months)).Item2;
            result.EcriChange12M = result.EcriLevel / ecriLevelBefore12Months - 1;

            var yearResults = EcriResults.Where(p => p.Date > dateTime.AddMonths(-Months) && p.Date <= dateTime).ToList();

            if (yearResults.Count < Months)
            {
                return false;
            }

            if (yearResults.Count > Months)
            {
                throw new InvalidOperationException($"Year data count should be no more then {Months} months");
            }

            result.EcriMovingAvg12 = yearResults.Average(r => r.EcriChange12M);
            var previuosResult = EcriResults.SingleOrDefault(r => r.Date.CompareByMonth(dateTime.AddMonths(-1)));

            if (previuosResult == null || previuosResult.EcriMovingAvg12 == 0)
            {
                return false;
            }

            result.EcriMovingAvgMom12 = result.EcriMovingAvg12 - previuosResult.EcriMovingAvg12;
            result.CyclePhase = DetectCyclePhase(result.EcriMovingAvg12, result.EcriMovingAvgMom12);
            result.CyclePhaseTwoMonthsOld = EcriResults.SingleOrDefault(e => e.Date == dateTime.AddMonths(-2)).CyclePhase;
            return true;
        }

        private static byte DetectCyclePhase(decimal ecriMovingAvg12, decimal ecriMovingAvgMom12)
        {
            if (ecriMovingAvg12 > 0 && ecriMovingAvgMom12 < 0)
                return 1;
            if (ecriMovingAvg12 < 0 && ecriMovingAvgMom12 < 0)
                return 2;
            if (ecriMovingAvg12 < 0 && ecriMovingAvgMom12 > 0)
                return 3;
            return 4;
        }

        private IEnumerable<(DateTime date, decimal level)> ReadDataFromEcriCsv()
        {
            var rows = File.ReadLines(EcriCsv);
            //First row is headers so skip it
            foreach (var row in rows.Skip(1))
            {
                if (row.Replace("\n", "").Trim() == "") continue;
                var cols = row.Split(',');

                var date = DateTime.ParseExact(cols[0], "dd-MMM-yy", CultureInfo.InvariantCulture);
                var level = Convert.ToDecimal(cols[1]);
                yield return (date, level);
            }
        }
    }
}
