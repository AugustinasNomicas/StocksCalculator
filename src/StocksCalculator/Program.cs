using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StocksCalculator.Services;

namespace StocksCalculator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Welcome to intelligent stocks calculator");
            var fin = new YahooFinanceService();

            Console.WriteLine("Getting data from yahoo...");

            Task.Run(async () =>
            {
                var result = await fin.DownloadDataAsync("SPY", new DateTime(2016, 1, 1), new DateTime(2017, 1, 1));
                result.ForEach(r => Console.WriteLine($"{r.Date} {r.AdjClose}"));

            }).Wait();

            Console.WriteLine("Done. Thanks. Go away.");
            Console.ReadKey();
        }
    }
}
