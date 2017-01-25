using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using StocksCalculator.Models;

namespace StocksCalculator.Services
{
    public class YahooFinanceService
    {
        private const string YahooUrl = "http://ichart.finance.yahoo.com/table.csv";

        public async Task<List<YahooHistoricalStock>> DownloadDataAsync(string ticker, int startYear, int endYear)
        {
            using (var web = new HttpClient())
            {
                var url = $"{YahooUrl}?s={ticker}&a=01&b=01" +
                          $"&c={startYear}&d=01&e=01&f={endYear}" +
                          "&g=m&ignore=.csv";
                
                var data = web.GetStringAsync(url);

                var result = data.ContinueWith(d => ParseCsv(d.Result));

                return await result;
            }
        }

        public List<YahooHistoricalStock> ReadDataFromFile(string filename)
        {
            var csvData = File.ReadAllText(filename);
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
                    Volume = Convert.ToDecimal(cols[5]),
                    AdjClose = Convert.ToDecimal(cols[6])
                };

                retval.Add(hs);
            }

            return retval;
        }
    }
}
