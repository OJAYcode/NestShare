using System.ComponentModel.DataAnnotations;
using Thrift.Models;

namespace Thrift.ViewModels
{    public class LoanIndexViewModel
    {
        public List<Loan> Loans { get; set; } = new();
        public List<Loan> ActiveLoans { get; set; } = new();
        public List<Loan> PaidOffLoans { get; set; } = new();
        public LoanSummaryViewModel Summary { get; set; } = new();
        public List<LoanPayment> RecentPayments { get; set; } = new();
        public List<LoanPayment> UpcomingPayments { get; set; } = new();
        
        // Summary properties
        public int TotalLoans { get; set; }
        public decimal TotalPrincipal { get; set; }
        public decimal TotalCurrentBalance { get; set; }
        public decimal TotalMonthlyPayments { get; set; }
        public decimal AverageInterestRate { get; set; }
    }public class LoanSummaryViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string LoanName { get; set; } = string.Empty;
        public LoanType LoanType { get; set; }
        public decimal PrincipalAmount { get; set; }
        public decimal InterestRate { get; set; }
        public int LoanTermMonths { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal MonthlyPayment { get; set; }
        public LoanStatus Status { get; set; }
        public DateTime? NextPaymentDate { get; set; }
        public decimal ProgressPercentage { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal RemainingBalance { get; set; }
        
        public decimal TotalOutstandingBalance { get; set; }
        public decimal TotalMonthlyPayments { get; set; }
        public decimal TotalInterestPaid { get; set; }
        public decimal TotalPrincipalPaid { get; set; }
        public int TotalActiveLoans { get; set; }
        public int TotalPaidOffLoans { get; set; }
        public decimal AverageInterestRate { get; set; }
        public DateTime? NextPaymentDue { get; set; }
        public decimal NextPaymentAmount { get; set; }
        public int DelinquentLoans { get; set; }
        public decimal DelinquentAmount { get; set; }
    }

    public class CreateLoanViewModel
    {
        [Required(ErrorMessage = "Loan name is required")]
        [StringLength(100, ErrorMessage = "Loan name cannot exceed 100 characters")]
        public string LoanName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Loan type is required")]
        public LoanType LoanType { get; set; }

        [Required(ErrorMessage = "Principal amount is required")]
        [Range(0.01, 10000000, ErrorMessage = "Principal amount must be between $0.01 and $10,000,000")]
        [Display(Name = "Principal Amount")]
        public decimal PrincipalAmount { get; set; }

        [Required(ErrorMessage = "Interest rate is required")]
        [Range(0, 50, ErrorMessage = "Interest rate must be between 0% and 50%")]
        [Display(Name = "Annual Interest Rate (%)")]
        public decimal InterestRate { get; set; }        [Required(ErrorMessage = "Interest type is required")]
        [Display(Name = "Interest Type")]
        public InterestType InterestType { get; set; } = InterestType.Fixed;        [Required(ErrorMessage = "Loan term is required")]
        [Range(1, 600, ErrorMessage = "Loan term must be between 1 and 600 months")]
        [Display(Name = "Loan Term (Months)")]
        public int TermInMonths { get; set; }

        public int LoanTermMonths { get; set; }

        [Required(ErrorMessage = "Payment frequency is required")]
        [Display(Name = "Payment Frequency")]
        public PaymentFrequency PaymentFrequency { get; set; } = PaymentFrequency.Monthly;

        [Display(Name = "Calculation Type")]
        public LoanCalculationType CalculationType { get; set; } = LoanCalculationType.StandardAmortization;

        [Required(ErrorMessage = "Start date is required")]
        [Display(Name = "Loan Start Date")]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Display(Name = "First Payment Date")]
        public DateTime? FirstPaymentDate { get; set; }

        [StringLength(100, ErrorMessage = "Lender name cannot exceed 100 characters")]
        public string? Lender { get; set; }

        [StringLength(50, ErrorMessage = "Loan number cannot exceed 50 characters")]
        [Display(Name = "Loan/Account Number")]
        public string? LoanNumber { get; set; }

        [Range(0, 10000, ErrorMessage = "Origination fee must be between $0 and $10,000")]
        [Display(Name = "Origination Fee")]
        public decimal? OriginationFee { get; set; }

        [Range(0, 50000, ErrorMessage = "Prepayment penalty must be between $0 and $50,000")]
        [Display(Name = "Prepayment Penalty")]
        public decimal? PrepaymentPenalty { get; set; }

        [Display(Name = "Allow Extra Payments")]
        public bool AllowExtraPayments { get; set; } = true;

        [Range(0, 10000, ErrorMessage = "Extra payment amount must be between $0 and $10,000")]
        [Display(Name = "Monthly Extra Payment")]
        public decimal? ExtraPaymentAmount { get; set; }

        [Range(300, 850, ErrorMessage = "Credit score must be between 300 and 850")]
        [Display(Name = "Credit Score")]
        public decimal? CreditScore { get; set; }

        [Range(0, 100, ErrorMessage = "Debt-to-income ratio must be between 0% and 100%")]
        [Display(Name = "Debt-to-Income Ratio (%)")]
        public decimal? DebtToIncomeRatio { get; set; }
    }

    public class EditLoanViewModel : CreateLoanViewModel
    {
        public string Id { get; set; } = string.Empty;
        public LoanStatus Status { get; set; }
        public decimal PaidToDate { get; set; }
        public decimal RemainingBalance { get; set; }
        public DateTime? LastPaymentDate { get; set; }
        public int? DaysDelinquent { get; set; }
    }    public class LoanDetailsViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string LoanName { get; set; } = string.Empty;
        public LoanType LoanType { get; set; }
        public LoanStatus Status { get; set; }
        public decimal PrincipalAmount { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal InterestRate { get; set; }
        public int TermInMonths { get; set; }
        public int LoanTermMonths { get; set; }
        public decimal MonthlyPayment { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? MaturityDate { get; set; }        public DateTime? NextPaymentDate { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalPayment { get; set; }
        public decimal RemainingBalance { get; set; }
        public decimal TotalInterest { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
        public decimal ProgressPercentage { get; set; }
        public int PaymentsMade { get; set; }
        public int RemainingPayments { get; set; }
        public int PaymentsRemaining { get; set; }
        public string? Description { get; set; }
        
        public List<LoanPayment> PaymentHistory { get; set; } = new();
        public PaymentSchedule? PaymentSchedule { get; set; }
        public LoanAnalyticsViewModel Analytics { get; set; } = new();
        public bool CanMakePayment { get; set; } = true;
        public decimal NextPaymentAmount { get; set; }
        public DateTime? NextPaymentDue { get; set; }
    }    public class LoanAnalyticsViewModel
    {
        // Summary metrics
        public int TotalLoans { get; set; }
        public int ActiveLoans { get; set; }
        public decimal TotalPrincipal { get; set; }
        public decimal TotalRemainingBalance { get; set; }
        public decimal TotalCurrentBalance { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalMonthlyPayments { get; set; }
        public decimal TotalPaidToDate { get; set; }
        public decimal TotalInterestPaid { get; set; }
        
        // Interest rate analysis
        public decimal AverageInterestRate { get; set; }
        public decimal WeightedAverageInterestRate { get; set; }
        public decimal HighestInterestRate { get; set; }
        public decimal LowestInterestRate { get; set; }
        
        // Payment analysis
        public decimal AveragePaymentSize { get; set; }
        public decimal OnTimePaymentPercentage { get; set; }
        
        // Distributions
        public Dictionary<string, int> StatusDistribution { get; set; } = new();
        public Dictionary<string, int> TypeDistribution { get; set; } = new();
        
        // Trends
        public List<PaymentTrendData> PaymentTrends { get; set; } = new();
        
        // Loan summaries
        public List<LoanSummary> LoanSummaries { get; set; } = new();
        
        // Legacy properties for backward compatibility
        public decimal TotalPrincipalPaid { get; set; }
        public decimal RemainingInterest { get; set; }
        public decimal PayoffProgress { get; set; }
        public int PaymentsMade { get; set; }
        public int PaymentsRemaining { get; set; }
        public DateTime? ProjectedPayoffDate { get; set; }
        public decimal? InterestSavingsFromExtra { get; set; }
        public int? MonthsSavedFromExtra { get; set; }
        public List<MonthlyPaymentBreakdown> PaymentBreakdown { get; set; } = new();
        public List<Loan> Loans { get; set; } = new();
        public List<LoanPayment> PaymentHistory { get; set; } = new();
        public Dictionary<LoanType, int> LoansByType { get; set; } = new();
        public Dictionary<LoanStatus, int> LoansByStatus { get; set; } = new();
        public decimal MonthlyPayments { get; set; }
    }

    public class PaymentTrendData
    {
        public string Month { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int PaymentCount { get; set; }
    }

    public class LoanSummary
    {
        public string Id { get; set; } = string.Empty;
        public string LoanName { get; set; } = string.Empty;
        public LoanType LoanType { get; set; }
        public LoanStatus Status { get; set; }
        public decimal InterestRate { get; set; }
        public decimal RemainingBalance { get; set; }
        public decimal ProgressPercentage { get; set; }
        public DateTime? NextDueDate { get; set; }
    }

    public class MonthlyPaymentBreakdown
    {
        public DateTime Date { get; set; }
        public decimal PrincipalAmount { get; set; }
        public decimal InterestAmount { get; set; }
        public decimal Balance { get; set; }
    }    public class MakePaymentViewModel
    {
        public string LoanId { get; set; } = string.Empty;
        public string LoanName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Payment amount is required")]
        [Range(0.01, 50000, ErrorMessage = "Payment amount must be between $0.01 and $50,000")]
        [Display(Name = "Payment Amount")]
        public decimal PaymentAmount { get; set; }

        [Range(0, 10000, ErrorMessage = "Extra payment must be between $0 and $10,000")]
        [Display(Name = "Extra Payment (Optional)")]
        public decimal ExtraPayment { get; set; } = 0;

        [Required(ErrorMessage = "Payment date is required")]
        [Display(Name = "Payment Date")]
        public DateTime PaymentDate { get; set; } = DateTime.Today;

        [StringLength(50, ErrorMessage = "Payment method cannot exceed 50 characters")]
        [Display(Name = "Payment Method")]
        public string? PaymentMethod { get; set; }

        [StringLength(100, ErrorMessage = "Transaction ID cannot exceed 100 characters")]
        [Display(Name = "Transaction ID")]
        public string? TransactionId { get; set; }

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }

        [Display(Name = "Set up automatic payments")]
        public bool SetupAutomaticPayment { get; set; } = false;        // Additional properties for display
        public decimal CurrentBalance { get; set; }
        public decimal ScheduledPaymentAmount { get; set; }
        public decimal MonthlyPayment { get; set; }
        public decimal InterestRate { get; set; }
        public DateTime? NextDueDate { get; set; }
        public DateTime? NextPaymentDate { get; set; }
        public int DaysUntilDue { get; set; }

        // Read-only properties for display
        public decimal RegularPaymentAmount { get; set; }
        public decimal PrincipalPortion { get; set; }
        public decimal InterestPortion { get; set; }
        public decimal RemainingBalance { get; set; }
        public DateTime DueDate { get; set; }
    }    public class LoanCalculatorViewModel
    {
        [Required(ErrorMessage = "Principal amount is required")]
        [Range(1, 10000000, ErrorMessage = "Principal amount must be between $1 and $10,000,000")]
        [Display(Name = "Loan Amount")]
        public decimal PrincipalAmount { get; set; }

        [Required(ErrorMessage = "Interest rate is required")]
        [Range(0.01, 50, ErrorMessage = "Interest rate must be between 0.01% and 50%")]
        [Display(Name = "Annual Interest Rate (%)")]
        public decimal InterestRate { get; set; }

        [Required(ErrorMessage = "Loan term is required")]
        [Range(1, 50, ErrorMessage = "Loan term must be between 1 and 50 years")]
        [Display(Name = "Loan Term (Years)")]
        public int LoanTermYears { get; set; }

        [Range(0, 11, ErrorMessage = "Additional months must be between 0 and 11")]
        [Display(Name = "Additional Months")]
        public int LoanTermMonths { get; set; } = 0;

        [Display(Name = "Payment Frequency")]
        public PaymentFrequency PaymentFrequency { get; set; } = PaymentFrequency.Monthly;

        [Required(ErrorMessage = "Interest type is required")]
        [Display(Name = "Interest Type")]
        public InterestType InterestType { get; set; } = InterestType.Fixed;

        [Range(0, 10000, ErrorMessage = "Extra payment must be between $0 and $10,000")]
        [Display(Name = "Extra Monthly Payment")]
        public decimal ExtraPayment { get; set; } = 0;

        [Display(Name = "Show Amortization Schedule")]
        public bool ShowAmortizationSchedule { get; set; } = false;

        // Calculation results
        public decimal MonthlyPayment { get; set; }
        public decimal TotalInterest { get; set; }
        public decimal TotalPayment { get; set; }
        public decimal TotalPayments { get; set; }
        public int TotalPaymentCount { get; set; }
        public DateTime? PayoffDate { get; set; }
        public decimal? InterestSavings { get; set; }
        public int? TimeSavingsMonths { get; set; }
        public List<ScheduledPayment> AmortizationSchedule { get; set; } = new();
    }

    public class LoanComparisonViewModel
    {
        public List<LoanCalculatorViewModel> Scenarios { get; set; } = new();
        public ComparisonResult? BestScenario { get; set; }
    }

    public class ComparisonResult
    {
        public int ScenarioIndex { get; set; }
        public string ScenarioName { get; set; } = string.Empty;
        public decimal TotalInterestSavings { get; set; }
        public int MonthsSaved { get; set; }
        public string Recommendation { get; set; } = string.Empty;
    }
}
