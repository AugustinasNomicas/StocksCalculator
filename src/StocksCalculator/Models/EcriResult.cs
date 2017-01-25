using System;
using System.Collections.Generic;

namespace StocksCalculator.Models
{
    public class EcriResult
    {
        public DateTime Date { get; set; }
        public decimal EcriLevel { get; set; }
        public decimal EcriChange12M { get; set; }
        public decimal EcriMovingAvg12 { get; set; }
        public decimal EcriMovingAvgMom12 { get; set; }
        public byte? CyclePhase { get; set; }
        public byte? CyclePhaseTwoMonthsOld { get; set; }

        public decimal StocksMovingAvg { get; set; }
        public decimal StocksReturn { get; set; }
        public bool StocksTrendFollowFilter { get; set; }
        public decimal BondsReturn { get; set; }

        public List<AvgReturnByCycle> StocksAvgReturnByCycle { get; set; }
        public List<AvgReturnByCycle> BondsAvgReturnByCycle { get; set; }

        public StrategyResult Result { get; set; }
    }

    public class AvgReturnByCycle
    {
        public byte CyclePhase { get; set; }
        public decimal AvgReturn { get; set; }
        public bool Result { get; set; }
    }
}
