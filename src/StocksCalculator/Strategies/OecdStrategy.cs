using StocksCalculator.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace StocksCalculator.Strategies
{
    public class OecdStrategy
    {
        private const int Months = 12;
        private const string OECDCsv = @"..\..\OECD.csv";

        public List<OecdResult> Result { get; set; } = new List<OecdResult>();

        public void Compute(List<StockPrice> prices, DateTime dateTime)
        {
            Result = ReadDataFromCsv().Select(r => new OecdResult { Date = r.date, OecdLevel = r.level })
                .ToList();
        }

        private IEnumerable<(DateTime date, decimal level)> ReadDataFromCsv()
        {
            var rows = File.ReadLines(OECDCsv);
            //First row is headers so skip it
            foreach (var row in rows.Skip(1))
            {
                if (row.Replace("\n", "").Trim() == "") continue;
                var cols = row.Split(',');

                var dateAsStr = cols[7].Replace("\"", "");
                var date = DateTime.ParseExact(dateAsStr, "MMM-yyyy", CultureInfo.InvariantCulture);
                var level = Convert.ToDecimal(cols[14]);
                yield return (date, level);
            }
        }
    }
}
