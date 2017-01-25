using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace StocksCalculator.Models
{
    [DebuggerDisplay("{Date.ToString()} Snp500: {Snp500} Bonds: {Bonds}")]
    public class StockPrice
    {
        public DateTime Date { get; set; }
        public decimal Snp500 { get; set; }
        public decimal Bonds { get; set; }
    }
}
