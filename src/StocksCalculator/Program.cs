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

            var calculator = new PortfolioCalculator();

            var result = calculator.Calculate(new Models.PortfolioCalculator.CalculateRequest
            {
                StocksTicker = Snp500Ticker,
                BondsTicker = BondsTicker,
                StartDate = new DateTime(2008,1,1),
                EndDate = new DateTime(2017, 1, 1),
                StartMoneyAmount = 5000,
                TransactionFee = 10
            });

            ConsoleTable.PrintLine();
            ConsoleTable.PrintRow("Date", "StockRatio", "BondsRatio");

            result.MonthResults.ForEach(m =>
            {
                ConsoleTable.PrintRow(m.Date.ToString(DateFormat), m.StocksRation.ToString("P"), m.BondsRation.ToString("P"));
            });

            Console.WriteLine("Done. Thanks. Go away.");
            Console.ReadKey();
        }
    }
}

