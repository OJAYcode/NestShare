namespace Thrift.Models
{
    public class MonthlyPaymentBreakdown
    {
        public DateTime Date { get; set; }
        public decimal PrincipalAmount { get; set; }
        public decimal InterestAmount { get; set; }
        public decimal Balance { get; set; }
    }
}
