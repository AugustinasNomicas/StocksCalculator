using System;
using System.Collections.Generic;
using System.Text;

namespace StocksCalculator.Models
{
    public class OecdResult : IStrategyResult
    {
        public DateTime Date { get; set; }
        public Decimal OecdLevel { get; set; }
        public Decimal OecdLevelChange { get; set; }

        public byte CyclePhase { get; set; }
        public byte? CyclePhaseTwoMonthsOld { get; set; }

        public decimal StocksMovingAvg { get; set; }
        public decimal StocksReturn { get; set; }
        public bool StocksTrendFollowFilter { get; set; }
        public decimal BondsReturn { get; set; }
        public List<AvgReturnByCycle> StocksAvgReturnByCycle { get; set; } = new List<AvgReturnByCycle>();
        public List<AvgReturnByCycle> BondsAvgReturnByCycle { get; set; } = new List<AvgReturnByCycle>();

        public StrategyResult Result { get; set; }
    }
}
