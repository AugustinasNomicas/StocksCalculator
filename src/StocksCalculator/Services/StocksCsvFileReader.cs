using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StocksCalculator.Models;

namespace StocksCalculator.Services
{
    public class StocksCsvFileReader : IHistoricalStocksData
    {
        private readonly string csvPath = @"..\..\csv\{0}.csv";

        public List<StockPrice> GetStockPrices(int sYear, int eYear, string snp500Ticker,
            string bondsTicker, bool dublicateLastMonth)
        {
            var snp500 = ReadDataFromFile(string.Format(csvPath, snp500Ticker)).Where(c => c.Date.Year >= sYear && c.Date.Year <= eYear);
            var bonds = ReadDataFromFile(string.Format(csvPath, bondsTicker)).Where(c => c.Date.Year >= sYear && c.Date.Year <= eYear);

            var stockPrices = snp500.Select(s => new StockPrice
            {
                Date = new DateTime(s.Date.Year, s.Date.Month, 1),
                Snp500 = s.AdjClose,
                Bonds = bonds.Last(b => b.Date.Year == s.Date.Year
                                        && b.Date.Month == s.Date.Month).AdjClose
            }).OrderBy(s => s.Date);

            // remove dublicates
            var distinctStockPrices = stockPrices.GroupBy(x => x.Date)
                .Select(g => g.Last()).ToList();

            var lastStockPrice = distinctStockPrices.Last();
            if (dublicateLastMonth) // this is used if we want to calculate on First day of the month, so current month is included into results
            {
                distinctStockPrices.Add(new StockPrice
                {
                    Date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
                    Snp500 = lastStockPrice.Snp500,
                    Bonds = lastStockPrice.Bonds
                });
            }
            return distinctStockPrices.ToList();
        }

        private List<YahooHistoricalStock> ReadDataFromFile(string filename)
        {
            var csvData = System.IO.File.ReadAllText(filename);
            return ParseCsv(csvData);
        }

        private static List<YahooHistoricalStock> ParseCsv(string data)
        {
            var retval = new List<YahooHistoricalStock>();
            data = data.Replace("r", "");

            var rows = data.Split('\n');

            //First row is headers so Ignore it
            for (var i = 1; i < rows.Length; i++)
            {
                if (rows[i].Replace("\n", "").Trim() == "") continue;

                var cols = rows[i].Split(',');

                var hs = new YahooHistoricalStock
                {
                    Date = Convert.ToDateTime(cols[0]),
                    Open = Convert.ToDecimal(cols[1]),
                    High = Convert.ToDecimal(cols[2]),
                    Low = Convert.ToDecimal(cols[3]),
                    Close = Convert.ToDecimal(cols[4]),
                    AdjClose = Convert.ToDecimal(cols[5]),
                    Volume = Convert.ToDecimal(cols[6])
                };

                retval.Add(hs);
            }

            return retval;
        }
    }
}
