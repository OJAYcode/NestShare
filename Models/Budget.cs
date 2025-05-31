using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Thrift.Models
{
    public class Budget
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        [BsonElement("Name")]
        public string Name { get; set; }
        
        [BsonElement("Description")]
        public string Description { get; set; }
        
        [BsonElement("Month")]
        public string Month { get; set; }
        
        [BsonElement("Year")]
        public int Year { get; set; }
        
        [BsonElement("Amount")]
        public decimal Amount { get; set; }
        
        [BsonElement("TotalIncome")]
        public decimal TotalIncome { get; set; }
        
        [BsonElement("AdditionalIncome")]
        public decimal AdditionalIncome { get; set; }
        
        [BsonElement("StartDate")]
        public DateTime StartDate { get; set; }
        
        [BsonElement("EndDate")]
        public DateTime EndDate { get; set; }
        
        [BsonElement("Category")]
        public string Category { get; set; }
        
        [BsonElement("UserId")]
        public string UserId { get; set; }
        
        [BsonElement("Expenses")]
        public List<Expense> Expenses { get; set; } = new List<Expense>();
        
        [BsonElement("IsRecurring")]
        public bool IsRecurring { get; set; }
        
        [BsonElement("RecurringPeriod")]
        public string RecurringPeriod { get; set; }
        
        [BsonElement("HousingExpenses")]
        public decimal HousingExpenses { get; set; }
        
        [BsonElement("UtilitiesExpenses")]
        public decimal UtilitiesExpenses { get; set; }
        
        [BsonElement("TransportationExpenses")]
        public decimal TransportationExpenses { get; set; }
        
        [BsonElement("FoodExpenses")]
        public decimal FoodExpenses { get; set; }
        
        [BsonElement("HealthcareExpenses")]
        public decimal HealthcareExpenses { get; set; }
        
        [BsonElement("EntertainmentExpenses")]
        public decimal EntertainmentExpenses { get; set; }
        
        [BsonElement("PersonalExpenses")]
        public decimal PersonalExpenses { get; set; }
        
        [BsonElement("SavingsAllocation")]
        public decimal SavingsAllocation { get; set; }
        
        [BsonElement("DebtPayment")]
        public decimal DebtPayment { get; set; }
        
        [BsonElement("OtherExpenses")]
        public decimal OtherExpenses { get; set; }
        
        [BsonElement("Notes")]
        public string Notes { get; set; }
        
        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        [BsonElement("UpdatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        
        // Calculated properties
        [BsonIgnore]
        public decimal TotalExpenses => 
            HousingExpenses + 
            UtilitiesExpenses + 
            TransportationExpenses + 
            FoodExpenses + 
            HealthcareExpenses + 
            EntertainmentExpenses + 
            PersonalExpenses + 
            SavingsAllocation + 
            DebtPayment + 
            OtherExpenses;
        
        [BsonIgnore]
        public decimal RemainingBudget => TotalIncome - TotalExpenses;
        
        [BsonIgnore]
        public decimal PercentageUsed => TotalIncome > 0 ? Math.Min(100, (TotalExpenses / TotalIncome) * 100) : 0;
        
        [BsonIgnore]
        public bool IsOverBudget => TotalExpenses > TotalIncome;
        
        [BsonIgnore]
        public int DaysRemaining => (EndDate - DateTime.Now).Days > 0 ? (EndDate - DateTime.Now).Days : 0;
    }
}