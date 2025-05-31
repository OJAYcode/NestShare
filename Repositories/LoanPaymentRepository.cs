using MongoDB.Driver;
using Thrift.Data;
using Thrift.Models;

namespace Thrift.Repositories
{
    public class LoanPaymentRepository
    {
        private readonly IMongoCollection<LoanPayment> _collection;
        private readonly IMongoCollection<PaymentSchedule> _scheduleCollection;
        
        public LoanPaymentRepository(MongoDbContext context)
        {
            _collection = context.GetCollection<LoanPayment>("LoanPayments");
            _scheduleCollection = context.GetCollection<PaymentSchedule>("PaymentSchedules");
        }
        
        public async Task<List<LoanPayment>> GetAllAsync(string userId)
        {
            return await _collection.Find(x => x.UserId == userId)
                .SortByDescending(x => x.PaymentDate)
                .ToListAsync();
        }
        
        public async Task<LoanPayment?> GetByIdAsync(string id, string userId)
        {
            return await _collection.Find(x => x.Id == id && x.UserId == userId)
                .FirstOrDefaultAsync();
        }
          public async Task CreateAsync(LoanPayment payment)
        {
            payment.CreatedAt = DateTime.UtcNow;
            payment.UpdatedAt = DateTime.UtcNow;
            await _collection.InsertOneAsync(payment);
        }

        public async Task CreatePaymentAsync(LoanPayment payment)
        {
            await CreateAsync(payment);
        }
        
        public async Task UpdateAsync(string id, LoanPayment payment)
        {
            payment.UpdatedAt = DateTime.UtcNow;
            await _collection.ReplaceOneAsync(x => x.Id == id, payment);
        }
        
        public async Task DeleteAsync(string id, string userId)
        {
            await _collection.DeleteOneAsync(x => x.Id == id && x.UserId == userId);
        }
        
        // Loan-specific payment queries
        public async Task<List<LoanPayment>> GetPaymentsByLoanAsync(string loanId, string userId)
        {
            return await _collection.Find(x => x.LoanId == loanId && x.UserId == userId)
                .SortByDescending(x => x.PaymentDate)
                .ToListAsync();
        }

        public async Task<List<LoanPayment>> GetPaymentsByLoanIdAsync(string loanId)
        {
            return await _collection.Find(x => x.LoanId == loanId)
                .SortByDescending(x => x.PaymentDate)
                .ToListAsync();
        }
        
        public async Task<List<LoanPayment>> GetPaymentsByStatusAsync(string userId, PaymentStatus status)
        {
            return await _collection.Find(x => x.UserId == userId && x.Status == status)
                .SortBy(x => x.DueDate)
                .ToListAsync();
        }
        
        public async Task<List<LoanPayment>> GetUpcomingPaymentsAsync(string userId, int daysAhead = 30)
        {
            var cutoffDate = DateTime.Now.AddDays(daysAhead);
            return await _collection.Find(x => x.UserId == userId && 
                x.Status == PaymentStatus.Pending && 
                x.DueDate <= cutoffDate)
                .SortBy(x => x.DueDate)
                .ToListAsync();
        }
        
        public async Task<List<LoanPayment>> GetOverduePaymentsAsync(string userId)
        {
            var today = DateTime.Now.Date;
            return await _collection.Find(x => x.UserId == userId && 
                x.Status == PaymentStatus.Pending && 
                x.DueDate < today)
                .SortBy(x => x.DueDate)
                .ToListAsync();
        }
        
        public async Task<List<LoanPayment>> GetRecentPaymentsAsync(string userId, int count = 10)
        {
            return await _collection.Find(x => x.UserId == userId && x.Status == PaymentStatus.Completed)
                .SortByDescending(x => x.PaymentDate)
                .Limit(count)
                .ToListAsync();
        }
        
        public async Task<LoanPayment?> GetLastPaymentForLoanAsync(string loanId, string userId)
        {
            return await _collection.Find(x => x.LoanId == loanId && x.UserId == userId && x.Status == PaymentStatus.Completed)
                .SortByDescending(x => x.PaymentDate)
                .FirstOrDefaultAsync();
        }
        
        public async Task<LoanPayment?> GetNextPaymentForLoanAsync(string loanId, string userId)
        {
            return await _collection.Find(x => x.LoanId == loanId && x.UserId == userId && x.Status == PaymentStatus.Pending)
                .SortBy(x => x.DueDate)
                .FirstOrDefaultAsync();
        }
        
        public async Task<decimal> GetTotalPaidForLoanAsync(string loanId, string userId)
        {
            var payments = await _collection.Find(x => x.LoanId == loanId && x.UserId == userId && x.Status == PaymentStatus.Completed)
                .ToListAsync();
            return payments.Sum(p => p.PaymentAmount + p.ExtraPayment);
        }
        
        public async Task<decimal> GetTotalInterestPaidForLoanAsync(string loanId, string userId)
        {
            var payments = await _collection.Find(x => x.LoanId == loanId && x.UserId == userId && x.Status == PaymentStatus.Completed)
                .ToListAsync();
            return payments.Sum(p => p.InterestAmount);
        }
        
        public async Task<decimal> GetTotalPrincipalPaidForLoanAsync(string loanId, string userId)
        {
            var payments = await _collection.Find(x => x.LoanId == loanId && x.UserId == userId && x.Status == PaymentStatus.Completed)
                .ToListAsync();
            return payments.Sum(p => p.PrincipalAmount);
        }
        
        public async Task<int> GetPaymentCountForLoanAsync(string loanId, string userId)
        {
            return (int)await _collection.CountDocumentsAsync(x => x.LoanId == loanId && x.UserId == userId && x.Status == PaymentStatus.Completed);
        }
        
        public async Task<List<LoanPayment>> GetPaymentsByDateRangeAsync(string userId, DateTime startDate, DateTime endDate)
        {
            return await _collection.Find(x => x.UserId == userId && 
                x.PaymentDate >= startDate && 
                x.PaymentDate <= endDate)
                .SortBy(x => x.PaymentDate)
                .ToListAsync();
        }
        
        public async Task<List<LoanPayment>> GetLatePaymentsAsync(string userId)
        {
            var today = DateTime.Now.Date;
            return await _collection.Find(x => x.UserId == userId && 
                x.Status == PaymentStatus.Late)
                .SortByDescending(x => x.DueDate)
                .ToListAsync();
        }
        
        public async Task<decimal> GetTotalLateFeesPaidAsync(string userId)
        {
            var payments = await _collection.Find(x => x.UserId == userId && x.LateFee > 0)
                .ToListAsync();
            return payments.Sum(p => p.LateFee);
        }
        
        // Payment Schedule Management
        public async Task<PaymentSchedule?> GetPaymentScheduleAsync(string loanId, string userId)
        {
            return await _scheduleCollection.Find(x => x.LoanId == loanId && x.UserId == userId)
                .FirstOrDefaultAsync();
        }
        
        public async Task CreatePaymentScheduleAsync(PaymentSchedule schedule)
        {
            schedule.CreatedAt = DateTime.UtcNow;
            schedule.UpdatedAt = DateTime.UtcNow;
            await _scheduleCollection.InsertOneAsync(schedule);
        }
        
        public async Task UpdatePaymentScheduleAsync(string scheduleId, PaymentSchedule schedule)
        {
            schedule.UpdatedAt = DateTime.UtcNow;
            await _scheduleCollection.ReplaceOneAsync(x => x.Id == scheduleId, schedule);
        }
          public async Task DeletePaymentScheduleAsync(string loanId, string userId)
        {
            await _scheduleCollection.DeleteOneAsync(x => x.LoanId == loanId && x.UserId == userId);
        }

        public async Task DeletePaymentScheduleByLoanIdAsync(string loanId)
        {
            await _scheduleCollection.DeleteOneAsync(x => x.LoanId == loanId);
        }

        public async Task<PaymentSchedule?> GetPaymentScheduleByLoanIdAsync(string loanId)
        {
            return await _scheduleCollection.Find(x => x.LoanId == loanId)
                .FirstOrDefaultAsync();
        }
        
        // Automatic payment methods
        public async Task<List<LoanPayment>> GetAutomaticPaymentsDueAsync(DateTime dueDate)
        {
            return await _collection.Find(x => x.IsAutomaticPayment && 
                x.Status == PaymentStatus.Pending && 
                x.ScheduledDate.HasValue && 
                x.ScheduledDate.Value.Date == dueDate.Date)
                .ToListAsync();
        }
        
        public async Task MarkPaymentAsProcessedAsync(string paymentId, string confirmationNumber)
        {
            var update = Builders<LoanPayment>.Update
                .Set(x => x.Status, PaymentStatus.Completed)
                .Set(x => x.ProcessedAt, DateTime.UtcNow)
                .Set(x => x.ConfirmationNumber, confirmationNumber)
                .Set(x => x.UpdatedAt, DateTime.UtcNow);
                
            await _collection.UpdateOneAsync(x => x.Id == paymentId, update);
        }
        
        public async Task MarkPaymentAsFailedAsync(string paymentId, string notes)
        {
            var update = Builders<LoanPayment>.Update
                .Set(x => x.Status, PaymentStatus.Failed)
                .Set(x => x.Notes, notes)
                .Set(x => x.UpdatedAt, DateTime.UtcNow);
                
            await _collection.UpdateOneAsync(x => x.Id == paymentId, update);
        }
    }
}