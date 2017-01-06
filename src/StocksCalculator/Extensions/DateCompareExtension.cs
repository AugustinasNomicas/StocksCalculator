using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StocksCalculator.Extensions
{
    public static class DateCompareExtension
    {
        public static bool CompareByMonth(this DateTime dt1, DateTime dt2)
        {
            return dt1.Year == dt2.Year && dt1.Month == dt2.Month;
        }
    }
}
