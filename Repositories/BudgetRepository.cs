using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using Thrift.Data;
using Thrift.Models;

namespace Thrift.Repositories
{
    public class BudgetRepository
    {
        private readonly IMongoCollection<Budget> _collection;
        
        public BudgetRepository(MongoDbContext context)
        {
            _collection = context.Budgets;
        }
        
        public async Task<List<Budget>> GetAllAsync(string userId)
        {
            return await _collection.Find(x => x.UserId == userId).ToListAsync();
        }
        
        public async Task<Budget> GetByIdAsync(string id)
        {
            return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        }
        
        public async Task CreateAsync(Budget budget)
        {
            await _collection.InsertOneAsync(budget);
        }
        
        public async Task UpdateAsync(string id, Budget budget)
        {
            await _collection.ReplaceOneAsync(x => x.Id == id, budget);
        }
        
        public async Task DeleteAsync(string id)
        {
            await _collection.DeleteOneAsync(x => x.Id == id);
        }
        
        // Specialized queries
        public async Task<List<Budget>> GetActiveBudgetsAsync(string userId)
        {
            return await _collection.Find(x => x.UserId == userId && x.EndDate >= DateTime.Now).ToListAsync();
        }
        
        public async Task<List<Budget>> GetBudgetsByCategoryAsync(string userId, string category)
        {
            return await _collection.Find(x => x.UserId == userId && x.Category == category).ToListAsync();
        }
    }
}