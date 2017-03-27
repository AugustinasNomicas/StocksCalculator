using System;
using System.Collections.Generic;
using System.Text;

namespace StocksCalculator.Models
{
    public class OecdResult
    {
        public DateTime Date { get; set; }
        public Decimal OecdLevel { get; set; }

        public StrategyResult Result { get; set; }
    }
}
