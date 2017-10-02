using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using StocksCalculator.Models;
using System.Linq;

namespace StocksCalculator.Services
{
    // YahooFinanceService is depreacated. Currently no known way to download stock quotes. 
    // Use StocksCsvFileReader to load data from CSV file

    //public class YahooFinanceService
    //{
    //    private const string YahooUrl = "http://ichart.finance.yahoo.com/quotes.csv";

    //    public List<StockPrice> GetStockPrices(int sYear, int eYear, string Snp500Ticker,
    //        string BondsTicker, bool loadFromFile = false) // load from file is used to check with Mokymai data
    //    {
    //        Console.WriteLine($"Getting data from yahoo from {sYear} to {eYear}");
    //        List<YahooHistoricalStock> snp500 = null;
    //        List<YahooHistoricalStock> bonds = null;

    //        Task.Run(async () =>
    //        {
    //            snp500 = await DownloadDataAsync(Snp500Ticker, sYear, eYear);
    //            bonds = !loadFromFile ? await DownloadDataAsync(BondsTicker, sYear, eYear)
    //            : ReadDataFromFile(@"C:\Users\nomicaug\Dropbox\Investavimas\Mokymai\Data\Vanguard Long-Term Treasury Inv (VUSTX).csv");
    //        }).Wait();

    //        return snp500.Select(s => new StockPrice
    //        {
    //            Date = new DateTime(s.Date.Year, s.Date.Month, 1),
    //            Snp500 = s.AdjClose,
    //            Bonds = bonds.Single(b => b.Date == s.Date).AdjClose
    //        }).OrderBy(s => s.Date).ToList();
    //    }

    //    public async Task<List<YahooHistoricalStock>> DownloadDataAsync(string ticker, int startYear, int endYear)
    //    {
    //        using (var web = new HttpClient())
    //        {
    //            int endMonth = 1;
    //            if (endYear == DateTime.Now.Year)
    //            {
    //                endMonth = DateTime.Now.Month;
    //            }

    //            var url = $"{YahooUrl}?s={ticker}&a=01&b=01" +
    //                      $"&c={startYear}&d={endMonth}&e=01&f={endYear}" +
    //                      "&g=m&ignore=.csv";

    //            var data = web.GetStringAsync(url);

    //            var result = data.ContinueWith(d => ParseCsv(d.Result));

    //            return await result;
    //        }
    //    }

    //    public List<YahooHistoricalStock> ReadDataFromFile(string filename)
    //    {
    //        var csvData = File.ReadAllText(filename);
    //        return ParseCsv(csvData);
    //    }

    //    private static List<YahooHistoricalStock> ParseCsv(string data)
    //    {
    //        var retval = new List<YahooHistoricalStock>();
    //        data = data.Replace("r", "");

    //        var rows = data.Split('\n');

    //        //First row is headers so Ignore it
    //        for (var i = 1; i < rows.Length; i++)
    //        {
    //            if (rows[i].Replace("\n", "").Trim() == "") continue;

    //            var cols = rows[i].Split(',');

    //            var hs = new YahooHistoricalStock
    //            {
    //                Date = Convert.ToDateTime(cols[0]),
    //                Open = Convert.ToDecimal(cols[1]),
    //                High = Convert.ToDecimal(cols[2]),
    //                Low = Convert.ToDecimal(cols[3]),
    //                Close = Convert.ToDecimal(cols[4]),
    //                Volume = Convert.ToDecimal(cols[5]),
    //                AdjClose = Convert.ToDecimal(cols[6])
    //            };

    //            retval.Add(hs);
    //        }

    //        return retval;
    //    }
    //}
}
