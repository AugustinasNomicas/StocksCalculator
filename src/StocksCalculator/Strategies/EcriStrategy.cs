using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using StocksCalculator.Extensions;
using StocksCalculator.Models;

namespace StocksCalculator.Strategies
{
    public class EcriStrategy
    {
        private const int Months = 12;
        private const string EcriCsv = @"..\..\ECRI.csv";

        public List<EcriResult> EcriResults { get; set; }

        public EcriStrategy()
        {
            EcriResults = new List<EcriResult>();
        }

        public void Compute(List<StockPrice> prices, DateTime dateTime)
        {
            var result = new EcriResult
            {
                Date = dateTime,
                Result = StrategyResult.None,
                StocksAvgReturnByCycle = new List<AvgReturnByCycle>()
            };

            // fill stock results with default values
            result.StocksAvgReturnByCycle.AddRange(
                Enumerable.Range(1, 4).Select(r => new AvgReturnByCycle
                { AvgReturn = 0, CyclePhase = (byte)r }));

            EcriResults.Add(result);
            var cyclePhaseComputed = ComputeCyclePhase(result, dateTime);
            

            if (!cyclePhaseComputed)
                return;

            var stocksComputed = ComputeStocks(prices, dateTime, result);
            if (!stocksComputed)
                return;


            result.Result = StrategyResult.Stocks; // change this
        }

        private bool ComputeStocks(List<StockPrice> prices, DateTime dateTime, EcriResult result)
        {
            result.StocksReturn = prices.Single(p => p.Date == dateTime).Snp500 
                / prices.Single(p => p.Date == dateTime.AddMonths(-1)).Snp500 - 1;

            result.StocksMovingAvg =
                prices.Where(p => p.Date > dateTime.AddMonths(-Months) && p.Date <= dateTime)
                .Select(s => s.Snp500)
                .Average();

            var months = 120; // 121

            months++;// inclusive period
            var ecri10YearData = EcriResults.Where(p =>
            p.Date >= dateTime.AddMonths(-months) && p.Date < dateTime).ToList();

            if (ecri10YearData.Count < months)
            {
                return false;
            }

            if (ecri10YearData.Any(e => e.CyclePhaseTwoMonthsOld == null))
            {
                return false;
            }

            if (ecri10YearData.Count > months)
            {
                throw new InvalidOperationException($"10 Year data count should be no more then {months} months");
            }

            result.StocksAvgReturnByCycle.Clear();
            for (byte cycle = 1; cycle <= 4; cycle++)
            {
                //var returnByCycle = ecri10YearData.Where(e => e.CyclePhaseTwoMonthsOld == cycle).Count();
                var filtered = ecri10YearData.Where(e => e.CyclePhaseTwoMonthsOld == cycle).ToList();
                var returnByCycle = filtered.Any() ? filtered.Average(r => r.StocksReturn) : -1;

                result.StocksAvgReturnByCycle.Add(new AvgReturnByCycle { AvgReturn = returnByCycle, CyclePhase = cycle });
            }

            return true;
        }

        private bool ComputeCyclePhase(EcriResult result, DateTime dateTime)
        {
            var ecriMonthlyData = ReadDataFromEcriCsv().GroupBy(w => new DateTime(w.Item1.Year, w.Item1.Month, 1))
                 .Select(m => m.Last()).Select(m => new Tuple<DateTime, double>(new DateTime(m.Item1.Year, m.Item1.Month, 1), m.Item2))
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

        private static byte DetectCyclePhase(double ecriMovingAvg12, double ecriMovingAvgMom12)
        {
            if (ecriMovingAvg12 > 0 && ecriMovingAvgMom12 < 0)
                return 1;
            if (ecriMovingAvg12 < 0 && ecriMovingAvgMom12 < 0)
                return 2;
            if (ecriMovingAvg12 < 0 && ecriMovingAvgMom12 > 0)
                return 3;
            return 4;
        }

        private IEnumerable<Tuple<DateTime, double>> ReadDataFromEcriCsv()
        {
            var rows = File.ReadLines(EcriCsv);
            //First row is headers so skip it
            foreach (var row in rows.Skip(1))
            {
                if (row.Replace("\n", "").Trim() == "") continue;
                var cols = row.Split(',');

                var date = DateTime.ParseExact(cols[0], "MMM-yy", CultureInfo.InvariantCulture);
                //var date = Convert.ToDateTime(cols[0]);
                var level = Convert.ToDouble(cols[1]);
                yield return new Tuple<DateTime, double>(date, level);
            }
        }
    }
}
