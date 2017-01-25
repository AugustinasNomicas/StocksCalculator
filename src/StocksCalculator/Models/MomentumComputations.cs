namespace StocksCalculator.Models
{
    public class MomentumComputations
    {
        public bool CanComputeResult { get; set; }
        public decimal Stocks12MonthMovingAverage { get; set; }
        public decimal Stocks3MonthMom { get; set; }
        public decimal Stocks6MonthMom { get; set; }
        public decimal Stocks12MonthMom { get; set; }
        public decimal StocksAverageMomentum { get; set; }
        public bool TffFilter { get; set; }

        public decimal Bonds3MonthMom { get; set; }
        public decimal Bonds6MonthMom { get; set; }
        public decimal Bonds12MonthMom { get; set; }
        public decimal BondsAverageMomentum { get; set; }
    }
}
