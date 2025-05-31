using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Thrift.Models
{
    public class Expense
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        [BsonElement("BudgetId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string BudgetId { get; set; }
        
        [BsonElement("Description")]
        public string Description { get; set; }
        
        [BsonElement("Amount")]
        public decimal Amount { get; set; }
        
        [BsonElement("Date")]
        public DateTime Date { get; set; } = DateTime.Now;
        
        [BsonElement("Category")]
        public string Category { get; set; }
        
        [BsonElement("UserId")]
        public string UserId { get; set; }
        
        [BsonElement("Vendor")]
        public string Vendor { get; set; }
        
        [BsonElement("IsRecurring")]
        public bool IsRecurring { get; set; }
        
        [BsonElement("RecurringFrequency")]
        public string RecurringFrequency { get; set; }
    }
}