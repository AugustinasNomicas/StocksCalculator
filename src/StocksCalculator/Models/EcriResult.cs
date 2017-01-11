using System;
using System.Collections.Generic;

namespace StocksCalculator.Models
{
    public class EcriResult
    {
        public DateTime Date { get; set; }
        public double EcriLevel { get; set; }
        public double EcriChange12M { get; set; }
        public double EcriMovingAvg12 { get; set; }
        public double EcriMovingAvgMom12 { get; set; }
        public byte? CyclePhase { get; set; }
        public byte? CyclePhaseTwoMonthsOld { get; set; }

        public double StocksMovingAvg { get; set; }
        public double StocksReturn { get; set; }
        public double BondsReturn { get; set; }

        public List<AvgReturnByCycle> StocksAvgReturnByCycle { get; set; }
        public List<AvgReturnByCycle> BondsAvgReturnByCycle { get; set; }

        public StrategyResult Result { get; set; }
    }

    public class AvgReturnByCycle
    {
        public byte CyclePhase { get; set; }
        public double AvgReturn { get; set; }
    }
}
