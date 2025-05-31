using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using Thrift.Data;
using Thrift.Models;

namespace Thrift.Repositories
{
    public class SavingsGoalRepository
    {
        private readonly IMongoCollection<SavingsGoal> _collection;
        
        public SavingsGoalRepository(MongoDbContext context)
        {
            _collection = context.SavingsGoals;
        }
        
        public async Task<List<SavingsGoal>> GetAllAsync(string userId)
        {
            return await _collection.Find(x => x.UserId == userId).ToListAsync();
        }
        
        public async Task<SavingsGoal> GetByIdAsync(string id)
        {
            return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        }
        
        public async Task CreateAsync(SavingsGoal goal)
        {
            await _collection.InsertOneAsync(goal);
        }
        
        public async Task UpdateAsync(string id, SavingsGoal goal)
        {
            await _collection.ReplaceOneAsync(x => x.Id == id, goal);
        }
        
        public async Task DeleteAsync(string id)
        {
            await _collection.DeleteOneAsync(x => x.Id == id);
        }
        
        // Add other specialized queries as needed
        public async Task<List<SavingsGoal>> GetActiveGoalsAsync(string userId)
        {
            return await _collection.Find(x => x.UserId == userId && x.Status == "Active").ToListAsync();
        }
        
        public async Task<List<SavingsGoal>> GetCompletedGoalsAsync(string userId)
        {
            return await _collection.Find(x => x.UserId == userId && x.Status == "Completed").ToListAsync();
        }
    }
}