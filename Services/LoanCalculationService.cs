using Thrift.Models;

namespace Thrift.Services
{
    public class LoanCalculationService
    {
        public decimal CalculateMonthlyPayment(decimal principal, decimal annualInterestRate, int termInMonths)
        {
            if (annualInterestRate == 0)
            {
                return principal / termInMonths;
            }

            decimal monthlyInterestRate = annualInterestRate / 12 / 100;
            double powerFactor = Math.Pow(1 + (double)monthlyInterestRate, termInMonths);
            
            decimal monthlyPayment = principal * monthlyInterestRate * (decimal)powerFactor / ((decimal)powerFactor - 1);
            
            return Math.Round(monthlyPayment, 2);
        }

        public PaymentSchedule GenerateAmortizationSchedule(Loan loan)
        {
            var schedule = new PaymentSchedule
            {
                LoanId = loan.Id,
                UserId = loan.UserId,
                IsActive = true
            };

            decimal balance = loan.PrincipalAmount;
            decimal monthlyInterestRate = loan.InterestRate / 12 / 100;
            decimal monthlyPayment = CalculateMonthlyPayment(loan.PrincipalAmount, loan.InterestRate, 
                loan.LoanTermYears * 12 + loan.LoanTermMonths);
            
            DateTime paymentDate = loan.FirstPaymentDate ?? loan.StartDate.AddMonths(1);
            int totalMonths = loan.LoanTermYears * 12 + loan.LoanTermMonths;
            
            for (int i = 1; i <= totalMonths; i++)
            {
                decimal interestPayment = balance * monthlyInterestRate;
                decimal principalPayment = monthlyPayment - interestPayment;
                
                if (principalPayment > balance)
                {
                    principalPayment = balance;
                    monthlyPayment = principalPayment + interestPayment;
                }
                
                balance -= principalPayment;
                
                // Handle the last payment rounding issue
                if (i == totalMonths && balance > 0 && balance < 0.1m)
                {
                    principalPayment += balance;
                    balance = 0;
                }
                
                var payment = new ScheduledPayment
                {
                    PaymentNumber = i,
                    DueDate = paymentDate,
                    PaymentDate = paymentDate,
                    PaymentAmount = monthlyPayment,
                    PrincipalAmount = principalPayment,
                    PrincipalPayment = principalPayment,
                    InterestAmount = interestPayment,
                    InterestPayment = interestPayment,
                    RemainingBalance = balance,
                    IsPaid = false
                };
                
                schedule.ScheduledPayments.Add(payment);
                
                // Adjust payment date based on frequency
                switch (loan.PaymentFrequency)
                {
                    case PaymentFrequency.Weekly:
                        paymentDate = paymentDate.AddDays(7);
                        break;
                    case PaymentFrequency.Biweekly:
                        paymentDate = paymentDate.AddDays(14);
                        break;
                    case PaymentFrequency.Monthly:
                        paymentDate = paymentDate.AddMonths(1);
                        break;
                    case PaymentFrequency.Quarterly:
                        paymentDate = paymentDate.AddMonths(3);
                        break;
                    case PaymentFrequency.SemiAnnually:
                        paymentDate = paymentDate.AddMonths(6);
                        break;
                    case PaymentFrequency.Annually:
                        paymentDate = paymentDate.AddYears(1);
                        break;
                    default:
                        paymentDate = paymentDate.AddMonths(1);
                        break;
                }
            }
            
            schedule.TotalPayments = schedule.ScheduledPayments.Count;
            
            return schedule;
        }

        public decimal CalculateTotalInterest(decimal principal, decimal monthlyPayment, int termInMonths)
        {
            decimal totalPayments = monthlyPayment * termInMonths;
            return totalPayments - principal;
        }

        public List<MonthlyPaymentBreakdown> GetPaymentBreakdown(Loan loan)
        {
            var result = new List<MonthlyPaymentBreakdown>();
            decimal balance = loan.PrincipalAmount;
            decimal monthlyInterestRate = loan.InterestRate / 12 / 100;
            decimal monthlyPayment = CalculateMonthlyPayment(loan.PrincipalAmount, loan.InterestRate, 
                loan.LoanTermYears * 12 + loan.LoanTermMonths);
            
            DateTime paymentDate = loan.FirstPaymentDate ?? loan.StartDate.AddMonths(1);
            int totalMonths = loan.LoanTermYears * 12 + loan.LoanTermMonths;
            
            for (int i = 1; i <= totalMonths; i++)
            {
                decimal interestPayment = balance * monthlyInterestRate;
                decimal principalPayment = monthlyPayment - interestPayment;
                
                if (principalPayment > balance)
                {
                    principalPayment = balance;
                    monthlyPayment = principalPayment + interestPayment;
                }
                
                balance -= principalPayment;
                
                result.Add(new MonthlyPaymentBreakdown
                {
                    Date = paymentDate,
                    PrincipalAmount = principalPayment,
                    InterestAmount = interestPayment,
                    Balance = balance
                });
                
                // Adjust payment date based on frequency
                switch (loan.PaymentFrequency)
                {
                    case PaymentFrequency.Weekly:
                        paymentDate = paymentDate.AddDays(7);
                        break;
                    case PaymentFrequency.Biweekly:
                        paymentDate = paymentDate.AddDays(14);
                        break;
                    case PaymentFrequency.Monthly:
                        paymentDate = paymentDate.AddMonths(1);
                        break;
                    case PaymentFrequency.Quarterly:
                        paymentDate = paymentDate.AddMonths(3);
                        break;
                    case PaymentFrequency.SemiAnnually:
                        paymentDate = paymentDate.AddMonths(6);
                        break;
                    case PaymentFrequency.Annually:
                        paymentDate = paymentDate.AddYears(1);
                        break;
                    default:
                        paymentDate = paymentDate.AddMonths(1);
                        break;
                }
            }
            
            return result;
        }
    }
}