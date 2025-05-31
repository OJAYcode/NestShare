using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using Thrift.Data;
using Thrift.Models;

namespace Thrift.Repositories
{
    public class UserRepository
    {
        private readonly IMongoCollection<User> _collection;
        
        public UserRepository(MongoDbContext context)
        {
            _collection = context.Users;
        }
        
        public async Task<User> GetByIdAsync(string id)
        {
            return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        }
        
        public async Task<User> GetByEmailAsync(string email)
        {
            return await _collection.Find(x => x.Email.ToLower() == email.ToLower()).FirstOrDefaultAsync();
        }
        
        public async Task CreateAsync(User user)
        {
            await _collection.InsertOneAsync(user);
        }
        
        public async Task UpdateAsync(string id, User user)
        {
            await _collection.ReplaceOneAsync(x => x.Id == id, user);
        }
        
        public async Task DeleteAsync(string id)
        {
            await _collection.DeleteOneAsync(x => x.Id == id);
        }
        
        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _collection.Find(x => x.Email.ToLower() == email.ToLower()).AnyAsync();
        }
        
        public async Task UpdatePasswordAsync(string id, string passwordHash)
        {
            var update = Builders<User>.Update.Set(u => u.PasswordHash, passwordHash);
            await _collection.UpdateOneAsync(x => x.Id == id, update);
        }
        
        public async Task UpdateLastLoginAsync(string id)
        {
            var update = Builders<User>.Update.Set(u => u.LastLogin, DateTime.Now);
            await _collection.UpdateOneAsync(x => x.Id == id, update);
        }
    }
}