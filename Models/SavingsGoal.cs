using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Thrift.Models
{
    public class SavingsGoal
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;
        
        [BsonElement("Name")]
        [Required(ErrorMessage = "Goal name is required")]
        [StringLength(100, ErrorMessage = "Goal name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;
        
        [BsonElement("Description")]
        public string? Description { get; set; }
        
        [BsonElement("TargetAmount")]
        [Required(ErrorMessage = "Target amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Target amount must be greater than 0")]
        public decimal TargetAmount { get; set; }
        
        [BsonElement("CurrentAmount")]
        public decimal CurrentAmount { get; set; }
        
        [BsonElement("StartDate")]
        public DateTime StartDate { get; set; } = DateTime.Now;
        
        [BsonElement("TargetDate")]
        public DateTime? TargetDate { get; set; }
        
        [BsonElement("Category")]
        [Required(ErrorMessage = "Category is required")]
        public string Category { get; set; } = string.Empty;
        
        [BsonElement("Status")]
        public string Status { get; set; } = "Active"; // Using string instead of enum
        
        [BsonElement("Priority")]
        public string Priority { get; set; } = "Medium"; // Using string instead of enum
        
        [BsonElement("Icon")]
        public string Icon { get; set; } = "piggy-bank";
        
        [BsonElement("ColorTheme")]
        public string ColorTheme { get; set; } = "purple";
        
        [BsonElement("UserId")]
        [Required(ErrorMessage = "User ID is required")]
        public string UserId { get; set; } = string.Empty;
        
        // Store transactions as embedded documents or references
        [BsonElement("Transactions")]
        public List<SavingsTransaction> Transactions { get; set; } = new List<SavingsTransaction>();
        
        [BsonElement("AutoSave")]
        public bool AutoSave { get; set; } = false;
        
        [BsonElement("Color")]
        public string Color { get; set; } = "#8B5CF6"; // Default purple color
        
        // Calculated properties
        [BsonIgnore]
        public decimal ProgressPercentage => TargetAmount > 0 ? Math.Min(100, (CurrentAmount / TargetAmount) * 100) : 0;
        
        [BsonIgnore]
        public bool IsCompleted => CurrentAmount >= TargetAmount;
        
        [BsonIgnore]
        public int? DaysRemaining => TargetDate.HasValue ? (int?)Math.Ceiling((TargetDate.Value - DateTime.Now).TotalDays) : null;
        
        [BsonIgnore]
        public decimal AmountRemaining => Math.Max(0, TargetAmount - CurrentAmount);
    }
}