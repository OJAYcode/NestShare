using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Thrift.Models;

namespace Thrift.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;
        public bool IsConnected { get; private set; } = false;
        
        public MongoDbContext(IConfiguration configuration)
        {
            try
            {
                var connectionString = configuration.GetConnectionString("MongoDbConnection");
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException("MongoDB connection string is not configured.");
                }
                
                var client = new MongoClient(connectionString);
                _database = client.GetDatabase("thrift_db");
                
                // Test the connection
                _database.RunCommandAsync((Command<MongoDB.Bson.BsonDocument>)"{ping:1}").Wait();
                IsConnected = true;
                Console.WriteLine("MongoDB connection successful");
            }
            catch (Exception ex)
            {
                IsConnected = false;
                Console.WriteLine($"MongoDB connection failed: {ex.Message}");
                // Still create the database object to avoid null reference exceptions
                // but operations will fail if attempted
                if (_database == null)
                {
                    var client = new MongoClient("mongodb://localhost:27017");
                    _database = client.GetDatabase("thrift_db");
                }
            }
        }
        
        // Collections
        public IMongoCollection<SavingsGoal> SavingsGoals => _database.GetCollection<SavingsGoal>("SavingsGoals");
        public IMongoCollection<SavingsTransaction> SavingsTransactions => _database.GetCollection<SavingsTransaction>("SavingsTransactions");
        public IMongoCollection<Budget> Budgets => _database.GetCollection<Budget>("Budgets");
        public IMongoCollection<Expense> Expenses => _database.GetCollection<Expense>("Expenses");
        public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
        public IMongoCollection<Loan> Loans => _database.GetCollection<Loan>("Loans");
        public IMongoCollection<LoanPayment> LoanPayments => _database.GetCollection<LoanPayment>("LoanPayments");
        public IMongoCollection<PaymentSchedule> PaymentSchedules => _database.GetCollection<PaymentSchedule>("PaymentSchedules");
        public IMongoCollection<AccountBalance> AccountBalances => _database.GetCollection<AccountBalance>("AccountBalances");
        
        // Generic collection access
        public IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            return _database.GetCollection<T>(collectionName);
        }
    }
}