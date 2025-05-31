using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Thrift.Data;

namespace Thrift.Pages
{
    public class CollectionStatus
    {
        public string Name { get; set; } = string.Empty;
        public bool Exists { get; set; }
        public long DocumentCount { get; set; }
    }

    public class MongoDbSetupModel : PageModel
    {
        private readonly MongoDbContext _mongoContext;
        private readonly IConfiguration _configuration;

        public bool IsConnected { get; set; }
        public string ConnectionString { get; set; } = string.Empty;
        public List<CollectionStatus> Collections { get; set; } = new List<CollectionStatus>();

        public MongoDbSetupModel(MongoDbContext mongoContext, IConfiguration configuration)
        {
            _mongoContext = mongoContext;
            _configuration = configuration;
        }

        public async Task OnGetAsync()
        {
            // Get connection status
            IsConnected = _mongoContext.IsConnected;
            
            // Get masked connection string
            var connString = _configuration.GetConnectionString("MongoDbConnection") ?? "Not configured";
            ConnectionString = MaskConnectionString(connString);

            // Check collections if connected
            if (IsConnected)
            {
                await CheckCollections();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!_mongoContext.IsConnected)
            {
                TempData["ErrorMessage"] = "Cannot initialize database - MongoDB is not connected";
                return RedirectToPage();
            }

            // Create necessary collections
            var collectionNames = new[] { "Loans", "LoanPayments", "PaymentSchedules", "Users", "SavingsGoals", "SavingsTransactions", "Budgets", "Expenses" };
            
            foreach (var collectionName in collectionNames)
            {
                try
                {
                    // Try to get the collection
                    var database = _mongoContext.GetType().GetProperty("_database")?.GetValue(_mongoContext) as IMongoDatabase;
                    if (database != null)
                    {
                        var filter = new MongoDB.Bson.BsonDocument();
                        var collection = database.GetCollection<MongoDB.Bson.BsonDocument>(collectionName);
                        
                        // Create index on UserId for common collections
                        if (collectionName != "Users")
                        {
                            var indexKeys = MongoDB.Bson.Serialization.BsonClassMap.LookupClassMap(typeof(MongoDB.Bson.BsonDocument))
                                .GetMemberMap("UserId")?.ElementName ?? "UserId";
                                
                            var indexModel = new CreateIndexModel<MongoDB.Bson.BsonDocument>(
                                Builders<MongoDB.Bson.BsonDocument>.IndexKeys.Ascending(indexKeys)
                            );
                            
                            await collection.Indexes.CreateOneAsync(indexModel);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating collection {collectionName}: {ex.Message}");
                }
            }

            TempData["SuccessMessage"] = "Database initialized successfully";
            return RedirectToPage();
        }

        private async Task CheckCollections()
        {
            var collectionNames = new[] { "Loans", "LoanPayments", "PaymentSchedules", "Users", "SavingsGoals", "SavingsTransactions", "Budgets", "Expenses" };
            
            foreach (var collectionName in collectionNames)
            {
                try
                {
                    var database = _mongoContext.GetType().GetProperty("_database")?.GetValue(_mongoContext) as IMongoDatabase;
                    if (database != null)
                    {
                        var filter = new MongoDB.Bson.BsonDocument();
                        var collection = database.GetCollection<MongoDB.Bson.BsonDocument>(collectionName);
                        
                        var exists = true;
                        long count = await collection.CountDocumentsAsync(filter);
                        
                        Collections.Add(new CollectionStatus
                        {
                            Name = collectionName,
                            Exists = exists,
                            DocumentCount = count
                        });
                    }
                }
                catch
                {
                    Collections.Add(new CollectionStatus
                    {
                        Name = collectionName,
                        Exists = false,
                        DocumentCount = 0
                    });
                }
            }
        }

        private string MaskConnectionString(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString) || connectionString == "Not configured")
                return connectionString;
            
            // Simple masking for display
            if (connectionString.Contains("@"))
            {
                var parts = connectionString.Split('@');
                if (parts.Length > 1)
                {
                    return $"mongodb://*****@{parts[1]}";
                }
            }
            
            if (connectionString.StartsWith("mongodb://"))
            {
                return "mongodb://localhost:27017/thrift_db";
            }
            
            return connectionString;
        }
    }
}
