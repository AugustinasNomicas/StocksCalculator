﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StocksCalculator.Models
{
    public class TrendFollowingResult
    {
        public decimal Average { get; set; }
        public StrategyResult Result { get; set; }
    }
}
