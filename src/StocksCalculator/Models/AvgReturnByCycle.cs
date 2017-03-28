using System;
using System.Collections.Generic;
using System.Text;

namespace StocksCalculator.Models
{
    public class AvgReturnByCycle
    {
        public byte CyclePhase { get; set; }
        public decimal AvgReturn { get; set; }
        public bool Result { get; set; }
    }
}
