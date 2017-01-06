namespace StocksCalculator.Models
{
    public class MomentumComputations
    {
        public bool CanComputeResult { get; set; }
        public double Stocks12MonthMovingAverage { get; set; }
        public double Stocks3MonthMom { get; set; }
        public double Stocks6MonthMom { get; set; }
        public double Stocks12MonthMom { get; set; }
        public double StocksAverageMomentum { get; set; }
        public bool TffFilter { get; set; }

        public double Bonds3MonthMom { get; set; }
        public double Bonds6MonthMom { get; set; }
        public double Bonds12MonthMom { get; set; }
        public double BondsAverageMomentum { get; set; }
    }
}
