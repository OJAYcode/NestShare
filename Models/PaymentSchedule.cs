using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Thrift.Models
{
    public class PaymentSchedule
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("userId")]
        [Required]
        public string UserId { get; set; } = string.Empty;

        [BsonElement("loanId")]
        [Required]
        public string LoanId { get; set; } = string.Empty;

        [BsonElement("scheduledPayments")]
        public List<ScheduledPayment> ScheduledPayments { get; set; } = new();

        [BsonElement("totalPayments")]
        public int TotalPayments { get; set; }

        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // LINQ Helper methods
        public bool Any() => ScheduledPayments.Any();
        
        public IEnumerable<ScheduledPayment> Where(Func<ScheduledPayment, bool> predicate)
        {
            return ScheduledPayments.Where(predicate);
        }
    }

    public class ScheduledPayment
    {
        [BsonElement("paymentNumber")]
        public int PaymentNumber { get; set; }

        [BsonElement("dueDate")]
        public DateTime DueDate { get; set; }

        [BsonElement("paymentDate")]
        public DateTime PaymentDate { get; set; }

        [BsonElement("paymentAmount")]
        public decimal PaymentAmount { get; set; }

        [BsonElement("principalAmount")]
        public decimal PrincipalAmount { get; set; }

        [BsonElement("principalPayment")]
        public decimal PrincipalPayment { get; set; }

        [BsonElement("interestAmount")]
        public decimal InterestAmount { get; set; }

        [BsonElement("interestPayment")]
        public decimal InterestPayment { get; set; }

        [BsonElement("remainingBalance")]
        public decimal RemainingBalance { get; set; }

        [BsonElement("extraPayment")]
        public decimal ExtraPayment { get; set; } = 0;

        [BsonElement("isPaid")]
        public bool IsPaid { get; set; } = false;        [BsonElement("paidDate")]
        public DateTime? PaidDate { get; set; }

        [BsonElement("actualAmountPaid")]
        public decimal? ActualAmountPaid { get; set; }
        
        [BsonElement("status")]
        public PaymentStatus Status { get; set; } = PaymentStatus.Scheduled;
    }
}
