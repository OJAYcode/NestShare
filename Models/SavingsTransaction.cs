using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Thrift.Models
{
    public class SavingsTransaction
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        [BsonElement("SavingsGoalId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string SavingsGoalId { get; set; }
        
        [BsonElement("Amount")]
        public decimal Amount { get; set; }
        
        [BsonElement("Type")]
        public string Type { get; set; } = "Deposit"; // Using string instead of enum
        
        [BsonElement("Note")]
        public string Note { get; set; }
        
        [BsonElement("TransactionDate")]
        public DateTime TransactionDate { get; set; } = DateTime.Now;
        
        [BsonElement("UserId")]
        public string UserId { get; set; }
    }
}