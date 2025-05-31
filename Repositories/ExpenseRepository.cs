using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using Thrift.Data;
using Thrift.Models;

namespace Thrift.Repositories
{
    public class ExpenseRepository
    {
        private readonly IMongoCollection<Expense> _collection;
        
        public ExpenseRepository(MongoDbContext context)
        {
            _collection = context.Expenses;
        }
        
        public async Task<List<Expense>> GetAllForBudgetAsync(string budgetId)
        {
            return await _collection.Find(x => x.BudgetId == budgetId).ToListAsync();
        }
        
        public async Task<List<Expense>> GetAllForUserAsync(string userId)
        {
            return await _collection.Find(x => x.UserId == userId).ToListAsync();
        }
        
        public async Task<Expense> GetByIdAsync(string id)
        {
            return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        }
        
        public async Task CreateAsync(Expense expense)
        {
            await _collection.InsertOneAsync(expense);
        }
        
        public async Task UpdateAsync(string id, Expense expense)
        {
            await _collection.ReplaceOneAsync(x => x.Id == id, expense);
        }
        
        public async Task DeleteAsync(string id)
        {
            await _collection.DeleteOneAsync(x => x.Id == id);
        }
        
        // Specialized queries
        public async Task<List<Expense>> GetRecentExpensesAsync(string userId, int limit = 10)
        {
            return await _collection.Find(x => x.UserId == userId)
                .SortByDescending(x => x.Date)
                .Limit(limit)
                .ToListAsync();
        }
        
        public async Task<List<Expense>> GetExpensesByDateRangeAsync(string userId, DateTime startDate, DateTime endDate)
        {
            return await _collection.Find(x => x.UserId == userId && x.Date >= startDate && x.Date <= endDate)
                .SortByDescending(x => x.Date)
                .ToListAsync();
        }
        
        public async Task<List<Expense>> GetExpensesByCategoryAsync(string userId, string category)
        {
            return await _collection.Find(x => x.UserId == userId && x.Category == category)
                .SortByDescending(x => x.Date)
                .ToListAsync();
        }
    }
}