using System.Collections.Generic;
using StocksCalculator.Models;

namespace StocksCalculator.Services
{
    public interface IHistoricalStocksData
    {
        List<StockPrice> GetStockPrices(int sYear, int eYear, string snp500File, string bondsFile, bool dublicateLastMonth);
    }
}