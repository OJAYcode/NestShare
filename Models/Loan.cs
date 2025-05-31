using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Thrift.Models
{
    public class Loan
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LoanName { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public LoanType LoanType { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Principal amount must be greater than 0")]
        public decimal PrincipalAmount { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "Interest rate must be between 0 and 100")]
        public decimal InterestRate { get; set; }

        public InterestType InterestType { get; set; } = InterestType.Fixed;

        [Required]
        [Range(1, 50, ErrorMessage = "Loan term must be between 1 and 50 years")]
        public int LoanTermYears { get; set; }

        [Range(0, 11, ErrorMessage = "Loan term months must be between 0 and 11")]
        public int LoanTermMonths { get; set; } = 0;

        [Required]
        public PaymentFrequency PaymentFrequency { get; set; } = PaymentFrequency.Monthly;

        public LoanCalculationType CalculationType { get; set; } = LoanCalculationType.StandardAmortization;

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? FirstPaymentDate { get; set; }

        public DateTime? MaturityDate { get; set; }

        [Required]
        public LoanStatus Status { get; set; } = LoanStatus.Active;        // Calculated fields
        public decimal MonthlyPayment { get; set; }
        public decimal TotalInterest { get; set; }
        public decimal TotalPayments { get; set; }
        public decimal TotalPayment { get; set; }
        public decimal RemainingBalance { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal PaidToDate { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal InterestPaidToDate { get; set; }
        public decimal PrincipalPaidToDate { get; set; }

        // Additional loan details
        public string? Lender { get; set; }
        public string? LoanNumber { get; set; }
        public decimal? OriginationFee { get; set; }
        public decimal? PrepaymentPenalty { get; set; }
        public bool AllowExtraPayments { get; set; } = true;
        public decimal? ExtraPaymentAmount { get; set; }        // Tracking
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastPaymentDate { get; set; }
        public DateTime? NextPaymentDue { get; set; }
        public DateTime? NextPaymentDate { get; set; }

        // Risk assessment
        public decimal? CreditScore { get; set; }
        public decimal? DebtToIncomeRatio { get; set; }
        public int? DaysDelinquent { get; set; } = 0;

        // Auto-calculation properties
        public int TotalPaymentCount 
        { 
            get 
            {                var totalMonths = (LoanTermYears * 12) + LoanTermMonths;
                return PaymentFrequency switch
                {
                    PaymentFrequency.Weekly => totalMonths * 4,
                    PaymentFrequency.Biweekly => totalMonths * 2,
                    PaymentFrequency.Monthly => totalMonths,
                    PaymentFrequency.Quarterly => totalMonths / 3,
                    PaymentFrequency.SemiAnnually => totalMonths / 6,
                    PaymentFrequency.Annually => totalMonths / 12,
                    _ => totalMonths
                };
            }
        }        public decimal PaymentProgress 
        { 
            get 
            {
                if (PrincipalAmount <= 0) return 0;
                return Math.Round((PrincipalPaidToDate / PrincipalAmount) * 100, 2);
            }
        }

        public decimal ProgressPercentage 
        { 
            get 
            {
                if (PrincipalAmount <= 0) return 0;
                return Math.Round((PrincipalPaidToDate / PrincipalAmount) * 100, 2);
            }
        }

        public int TermInMonths
        {
            get
            {
                return (LoanTermYears * 12) + LoanTermMonths;
            }
        }

        public bool IsDelinquent => DaysDelinquent > 30;
        public bool IsNearMaturity => MaturityDate.HasValue && MaturityDate.Value.Subtract(DateTime.Now).TotalDays <= 90;

        // Additional calculated properties
        public int PaymentsRemaining
        {
            get
            {
                var totalPayments = TotalPaymentCount;
                var paymentsMade = (int)Math.Round(PrincipalPaidToDate / (MonthlyPayment > 0 ? MonthlyPayment : 1));
                return Math.Max(0, totalPayments - paymentsMade);
            }
        }

        public string RiskLevel
        {
            get
            {
                var score = 0;
                
                // Credit score factor
                if (CreditScore.HasValue)
                {
                    if (CreditScore >= 750) score += 0;
                    else if (CreditScore >= 700) score += 1;
                    else if (CreditScore >= 650) score += 2;
                    else score += 3;
                }
                
                // Debt-to-income ratio factor
                if (DebtToIncomeRatio.HasValue)
                {
                    if (DebtToIncomeRatio <= 20) score += 0;
                    else if (DebtToIncomeRatio <= 35) score += 1;
                    else if (DebtToIncomeRatio <= 50) score += 2;
                    else score += 3;
                }
                
                // Delinquency factor
                if (DaysDelinquent > 0)
                {
                    if (DaysDelinquent <= 30) score += 1;
                    else if (DaysDelinquent <= 60) score += 2;
                    else score += 3;
                }
                
                return score switch
                {
                    <= 2 => "Low",
                    <= 4 => "Medium",
                    _ => "High"
                };
            }
        }
    }
}