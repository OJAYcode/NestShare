using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using Thrift.Data;
using Thrift.Models;

namespace Thrift.Repositories
{
    public class SavingsTransactionRepository
    {
        private readonly IMongoCollection<SavingsTransaction> _collection;
        
        public SavingsTransactionRepository(MongoDbContext context)
        {
            _collection = context.SavingsTransactions;
        }
        
        public async Task<List<SavingsTransaction>> GetAllForGoalAsync(string goalId)
        {
            return await _collection.Find(x => x.SavingsGoalId == goalId).ToListAsync();
        }
        
        public async Task<SavingsTransaction> GetByIdAsync(string id)
        {
            return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        }
        
        public async Task CreateAsync(SavingsTransaction transaction)
        {
            await _collection.InsertOneAsync(transaction);
        }
        
        public async Task UpdateAsync(string id, SavingsTransaction transaction)
        {
            await _collection.ReplaceOneAsync(x => x.Id == id, transaction);
        }
        
        public async Task DeleteAsync(string id)
        {
            await _collection.DeleteOneAsync(x => x.Id == id);
        }
        
        // Add other specialized queries
        public async Task<List<SavingsTransaction>> GetRecentTransactionsAsync(string userId, int limit = 10)
        {
            return await _collection.Find(x => x.UserId == userId)
                .SortByDescending(x => x.TransactionDate)
                .Limit(limit)
                .ToListAsync();
        }

        // Add this method if it doesn't exist already
        public async Task<List<SavingsTransaction>> GetAllForUserAsync(string userId)
        {
            return await _collection.Find(x => x.UserId == userId).ToListAsync();
        }
    }
}