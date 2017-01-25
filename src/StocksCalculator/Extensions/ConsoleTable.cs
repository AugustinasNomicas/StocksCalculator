using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace StocksCalculator.Extensions
{
    public  static class ConsoleTable
    {
        static int tableWidth = 190;

        public static void PrintLine()
        {
            Console.WriteLine(new string('-', tableWidth));
        }

        public static void PrintRow(params object[] columns)
        {
            int width = (tableWidth - columns.Length) / columns.Length;
            string row = "|";

            foreach (var column in columns)
            {
                decimal colAsdecimal = 0;
                if (decimal.TryParse(column?.ToString() ?? "0", out colAsdecimal))
                {
                    row += AlignCentre(Math.Round(colAsdecimal, 3).ToString(CultureInfo.InvariantCulture), width) + "|";
                }
                else
                {
                    row += AlignCentre(column.ToString(), width) + "|";
                }
                
            }

            Console.WriteLine(row);
        }

        private static string AlignCentre(string text, int width)
        {
            text = text.Length > width ? text.Substring(0, width - 3) + "..." : text;

            if (string.IsNullOrEmpty(text))
            {
                return new string(' ', width);
            }
            else
            {
                return text.PadRight(width - (width - text.Length) / 2).PadLeft(width);
            }
        }
    }
}
