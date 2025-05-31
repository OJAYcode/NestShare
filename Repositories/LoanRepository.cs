using MongoDB.Driver;
using Thrift.Data;
using Thrift.Models;

namespace Thrift.Repositories
{
    public class LoanRepository
    {
        private readonly IMongoCollection<Loan> _collection;
        
        public LoanRepository(MongoDbContext context)
        {
            _collection = context.GetCollection<Loan>("Loans");
        }
          public async Task<List<Loan>> GetAllAsync(string userId)
        {
            return await _collection.Find(x => x.UserId == userId)
                .SortByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Loan>> GetLoansByUserIdAsync(string userId)
        {
            return await GetAllAsync(userId);
        }
        
        public async Task<Loan?> GetByIdAsync(string id, string userId)
        {
            return await _collection.Find(x => x.Id == id && x.UserId == userId)
                .FirstOrDefaultAsync();
        }

        public async Task<Loan?> GetLoanByIdAsync(string id)
        {
            return await _collection.Find(x => x.Id == id)
                .FirstOrDefaultAsync();
        }
          public async Task CreateAsync(Loan loan)
        {
            try
            {
                loan.CreatedAt = DateTime.UtcNow;
                loan.UpdatedAt = DateTime.UtcNow;
                await _collection.InsertOneAsync(loan);
            }
            catch (Exception ex)
            {
                // Generate a more descriptive error
                string errorMessage = $"Failed to create loan: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $" Inner exception: {ex.InnerException.Message}";
                }
                Console.WriteLine(errorMessage);
                throw new Exception($"Database error: {errorMessage}", ex);
            }
        }

        public async Task CreateLoanAsync(Loan loan)
        {
            await CreateAsync(loan);
        }
        
        public async Task UpdateAsync(string id, Loan loan)
        {
            loan.UpdatedAt = DateTime.UtcNow;
            await _collection.ReplaceOneAsync(x => x.Id == id, loan);
        }

        public async Task UpdateLoanAsync(Loan loan)
        {
            await UpdateAsync(loan.Id, loan);
        }
        
        public async Task DeleteAsync(string id, string userId)
        {
            await _collection.DeleteOneAsync(x => x.Id == id && x.UserId == userId);
        }

        public async Task DeleteLoanAsync(string id)
        {
            await _collection.DeleteOneAsync(x => x.Id == id);
        }
        
        // Specialized queries
        public async Task<List<Loan>> GetActiveLoansByUserAsync(string userId)
        {
            return await _collection.Find(x => x.UserId == userId && x.Status == LoanStatus.Active)
                .SortBy(x => x.NextPaymentDue)
                .ToListAsync();
        }
        
        public async Task<List<Loan>> GetLoansByStatusAsync(string userId, LoanStatus status)
        {
            return await _collection.Find(x => x.UserId == userId && x.Status == status)
                .SortByDescending(x => x.UpdatedAt)
                .ToListAsync();
        }
        
        public async Task<List<Loan>> GetLoansByTypeAsync(string userId, LoanType loanType)
        {
            return await _collection.Find(x => x.UserId == userId && x.LoanType == loanType)
                .SortByDescending(x => x.CreatedAt)
                .ToListAsync();
        }
        
        public async Task<List<Loan>> GetDelinquentLoansAsync(string userId)
        {
            return await _collection.Find(x => x.UserId == userId && 
                x.Status == LoanStatus.Active && 
                x.DaysDelinquent > 0)
                .SortByDescending(x => x.DaysDelinquent)
                .ToListAsync();
        }
        
        public async Task<List<Loan>> GetLoansNearMaturityAsync(string userId, int daysAhead = 90)
        {
            var cutoffDate = DateTime.Now.AddDays(daysAhead);
            return await _collection.Find(x => x.UserId == userId && 
                x.Status == LoanStatus.Active && 
                x.MaturityDate.HasValue && 
                x.MaturityDate <= cutoffDate)
                .SortBy(x => x.MaturityDate)
                .ToListAsync();
        }
        
        public async Task<decimal> GetTotalOutstandingBalanceAsync(string userId)
        {
            var activeLoans = await GetActiveLoansByUserAsync(userId);
            return activeLoans.Sum(l => l.RemainingBalance);
        }
        
        public async Task<decimal> GetTotalMonthlyPaymentsAsync(string userId)
        {
            var activeLoans = await GetActiveLoansByUserAsync(userId);
            return activeLoans.Sum(l => l.MonthlyPayment + (l.ExtraPaymentAmount ?? 0));
        }
        
        public async Task<List<Loan>> GetLoansWithUpcomingPaymentsAsync(string userId, int daysAhead = 30)
        {
            var cutoffDate = DateTime.Now.AddDays(daysAhead);
            return await _collection.Find(x => x.UserId == userId && 
                x.Status == LoanStatus.Active && 
                x.NextPaymentDue.HasValue && 
                x.NextPaymentDue <= cutoffDate)
                .SortBy(x => x.NextPaymentDue)
                .ToListAsync();
        }
        
        public async Task UpdateLoanBalanceAsync(string loanId, decimal newBalance, DateTime lastPaymentDate)
        {
            var update = Builders<Loan>.Update
                .Set(x => x.RemainingBalance, newBalance)
                .Set(x => x.LastPaymentDate, lastPaymentDate)
                .Set(x => x.UpdatedAt, DateTime.UtcNow);
                
            await _collection.UpdateOneAsync(x => x.Id == loanId, update);
        }
        
        public async Task UpdateLoanStatusAsync(string loanId, LoanStatus status)
        {
            var update = Builders<Loan>.Update
                .Set(x => x.Status, status)
                .Set(x => x.UpdatedAt, DateTime.UtcNow);
                
            await _collection.UpdateOneAsync(x => x.Id == loanId, update);
        }
        
        public async Task<long> GetLoanCountByStatusAsync(string userId, LoanStatus status)
        {
            return await _collection.CountDocumentsAsync(x => x.UserId == userId && x.Status == status);
        }
        
        public async Task<List<Loan>> SearchLoansAsync(string userId, string searchTerm)
        {
            var filter = Builders<Loan>.Filter.And(
                Builders<Loan>.Filter.Eq(x => x.UserId, userId),
                Builders<Loan>.Filter.Or(
                    Builders<Loan>.Filter.Regex(x => x.LoanName, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                    Builders<Loan>.Filter.Regex(x => x.Lender, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                    Builders<Loan>.Filter.Regex(x => x.LoanNumber, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"))
                )
            );
            
            return await _collection.Find(filter)
                .SortByDescending(x => x.UpdatedAt)
                .ToListAsync();
        }
    }
}