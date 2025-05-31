using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Thrift.Models
{
    public class LoanPayment
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [Required]
        public string LoanId { get; set; } = string.Empty;
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        public decimal PaymentAmount { get; set; }
        
        public decimal ExtraPayment { get; set; } = 0;
        
        public decimal PrincipalPortion { get; set; }
        public decimal PrincipalAmount { get; set; }
        
        public decimal InterestPortion { get; set; }
        public decimal InterestAmount { get; set; }
        
        [Required]
        public DateTime PaymentDate { get; set; }
        
        public DateTime DueDate { get; set; }
        public DateTime? ScheduledDate { get; set; }
        
        public string? PaymentMethod { get; set; }
          public string? TransactionId { get; set; }
        
        public string? Notes { get; set; }
        
        public string? ConfirmationNumber { get; set; }
        
        public PaymentStatus Status { get; set; } = PaymentStatus.Scheduled;
        
        public bool IsAutomaticPayment { get; set; } = false;
          public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedDate { get; set; }
        public DateTime? ProcessedAt { get; set; }
        
        public decimal RemainingBalanceAfterPayment { get; set; }
        public decimal LateFee { get; set; } = 0;
    }
}