using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Thrift.Models
{
    public class ThriftGroup
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [Required]
        [BsonElement("Name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("Description")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [BsonElement("CreatedBy")]
        public string CreatedBy { get; set; } = string.Empty; // User ID of creator

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("UpdatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("IsActive")]
        public bool IsActive { get; set; } = true;

        [BsonElement("GroupCode")]
        public string GroupCode { get; set; } = string.Empty; // Unique code for joining

        [BsonElement("MaxMembers")]
        public int MaxMembers { get; set; } = 50;

        [BsonElement("IsPrivate")]
        public bool IsPrivate { get; set; } = false;

        [BsonElement("GroupType")]
        public GroupType GroupType { get; set; } = GroupType.Savings;

        [BsonElement("TargetAmount")]
        public decimal TargetAmount { get; set; } = 0;

        [BsonElement("CurrentAmount")]
        public decimal CurrentAmount { get; set; } = 0;        [BsonElement("ContributionFrequency")]
        public ContributionFrequency ContributionFrequency { get; set; } = ContributionFrequency.Monthly;

        [BsonElement("ContributionAmount")]
        public decimal ContributionAmount { get; set; } = 0;

        [BsonElement("MinimumContribution")]
        public decimal MinimumContribution { get; set; } = 0;

        [BsonElement("MaximumContribution")]
        public decimal MaximumContribution { get; set; } = 0;

        [BsonElement("StartDate")]
        public DateTime StartDate { get; set; } = DateTime.Today.AddDays(1);

        [BsonElement("EndDate")]
        public DateTime? EndDate { get; set; }

        [BsonElement("RequireApproval")]
        public bool RequireApproval { get; set; } = false;

        [BsonElement("PayoutDate")]
        public DateTime? PayoutDate { get; set; }

        [BsonElement("Rules")]
        public string Rules { get; set; } = string.Empty;

        [BsonElement("GroupImage")]
        public string? GroupImage { get; set; }        // Navigation properties (not stored in MongoDB)
        [BsonIgnore]
        public List<ThriftGroupMember> Members { get; set; } = new List<ThriftGroupMember>();

        [BsonIgnore]
        public List<GroupTransaction> Transactions { get; set; } = new List<GroupTransaction>();

        [BsonIgnore]
        public int MemberCount { get; set; } = 0;

        [BsonIgnore]
        public string CreatedByName { get; set; } = string.Empty;

        [BsonIgnore]
        public DateTime CreatedDate => CreatedAt; // Alias for compatibility
    }

    public class ThriftGroupMember
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [Required]
        [BsonElement("GroupId")]
        public string GroupId { get; set; } = string.Empty;

        [Required]
        [BsonElement("UserId")]
        public string UserId { get; set; } = string.Empty;

        [BsonElement("JoinedAt")]
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("Role")]
        public GroupRole Role { get; set; } = GroupRole.Member;

        [BsonElement("IsActive")]
        public bool IsActive { get; set; } = true;

        [BsonElement("TotalContributions")]
        public decimal TotalContributions { get; set; } = 0;

        [BsonElement("LastContributionDate")]
        public DateTime? LastContributionDate { get; set; }

        [BsonElement("PayoutPosition")]
        public int PayoutPosition { get; set; } = 0; // For rotating savings

        [BsonElement("HasReceivedPayout")]
        public bool HasReceivedPayout { get; set; } = false;

        // Navigation properties
        [BsonIgnore]
        public string UserName { get; set; } = string.Empty;

        [BsonIgnore]
        public string UserEmail { get; set; } = string.Empty;
    }

    public class GroupTransaction
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [Required]
        [BsonElement("GroupId")]
        public string GroupId { get; set; } = string.Empty;

        [Required]
        [BsonElement("UserId")]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [BsonElement("Amount")]
        public decimal Amount { get; set; }

        [BsonElement("TransactionType")]
        public GroupTransactionType TransactionType { get; set; } = GroupTransactionType.Contribution;

        [BsonElement("Description")]
        public string Description { get; set; } = string.Empty;

        [BsonElement("TransactionDate")]
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        [BsonElement("Status")]
        public TransactionStatus Status { get; set; } = TransactionStatus.Completed;

        [BsonElement("PaymentMethod")]
        public string? PaymentMethod { get; set; }

        [BsonElement("ReferenceNumber")]
        public string? ReferenceNumber { get; set; }

        [BsonElement("RecipientId")]
        public string? RecipientId { get; set; } // For transfers

        [BsonElement("Notes")]
        public string? Notes { get; set; }

        // Navigation properties
        [BsonIgnore]
        public string UserName { get; set; } = string.Empty;

        [BsonIgnore]
        public string RecipientName { get; set; } = string.Empty;
    }

    public class GroupInvitation
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [Required]
        [BsonElement("GroupId")]
        public string GroupId { get; set; } = string.Empty;

        [Required]
        [BsonElement("InvitedBy")]
        public string InvitedBy { get; set; } = string.Empty;

        [BsonElement("InvitedEmail")]
        public string? InvitedEmail { get; set; }

        [BsonElement("InvitedUserId")]
        public string? InvitedUserId { get; set; }

        [BsonElement("InvitationCode")]
        public string InvitationCode { get; set; } = string.Empty;

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("ExpiresAt")]
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(7);

        [BsonElement("Status")]
        public InvitationStatus Status { get; set; } = InvitationStatus.Pending;

        [BsonElement("Message")]
        public string? Message { get; set; }        // Navigation properties
        [BsonIgnore]
        public string GroupName { get; set; } = string.Empty;

        [BsonIgnore]
        public string InvitedByName { get; set; } = string.Empty;

        [BsonIgnore]
        public int DaysUntilExpiry => (int)(ExpiresAt - DateTime.UtcNow).TotalDays;
    }
}
