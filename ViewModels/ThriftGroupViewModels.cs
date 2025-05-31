using System.ComponentModel.DataAnnotations;
using System.Linq;
using Thrift.Models;

namespace Thrift.ViewModels
{
    public class ThriftGroupIndexViewModel
    {
        public List<ThriftGroup> UserGroups { get; set; } = new();
        public List<ThriftGroup> PublicGroups { get; set; } = new();
        public List<GroupInvitation> UserInvitations { get; set; } = new();
        public int TotalUserGroups => UserGroups.Count;
        public int TotalInvitations => UserInvitations.Count;
    }    public class ThriftGroupDetailsViewModel
    {
        public ThriftGroup Group { get; set; } = new();
        public List<ThriftGroupMember> Members { get; set; } = new();
        public List<GroupTransaction> Transactions { get; set; } = new();
        public bool IsMember { get; set; }
        public ThriftGroupMember? UserMembership { get; set; }
        
        // Properties required by the view
        public bool IsUserMember => IsMember;
        public bool IsUserAdmin => UserMembership?.Role == GroupRole.Creator || UserMembership?.Role == GroupRole.Admin;
        public List<GroupTransaction> RecentTransactions => Transactions.OrderByDescending(t => t.TransactionDate).Take(10).ToList();
        
        // Computed properties
        public bool CanEdit => UserMembership?.Role == GroupRole.Creator || UserMembership?.Role == GroupRole.Admin;
        public bool CanInvite => IsMember && (UserMembership?.Role == GroupRole.Creator || UserMembership?.Role == GroupRole.Admin);
        public bool CanContribute => IsMember && UserMembership?.IsActive == true;
        public bool IsGroupFull => Members.Count >= Group.MaxMembers;
        public decimal TotalContributions => Transactions.Where(t => t.TransactionType == GroupTransactionType.Contribution).Sum(t => t.Amount);
        public decimal AverageContribution => Members.Count > 0 ? TotalContributions / Members.Count : 0;
        public DateTime? LastActivity => Transactions.OrderByDescending(t => t.TransactionDate).FirstOrDefault()?.TransactionDate;
    }public class ThriftGroupSearchViewModel
    {
        public string? SearchTerm { get; set; }
        public GroupType? GroupType { get; set; }
        public ContributionFrequency? ContributionFrequency { get; set; }
        public string? SortBy { get; set; } = "CreatedAt";
        public decimal? MinTargetAmount { get; set; }
        public decimal? MaxTargetAmount { get; set; }
        public int? MinMembers { get; set; }
        public int? MaxMembers { get; set; }
        public bool HasAvailableSlots { get; set; } = false;
        public bool OnlyActive { get; set; } = true;
        public List<ThriftGroup> Groups { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public int TotalPages => (int)Math.Ceiling((double)Groups.Count / PageSize);
        public bool HasNextPage => CurrentPage < TotalPages;
        public bool HasPreviousPage => CurrentPage > 1;
    }

    public class CreateThriftGroupViewModel
    {
        [Required(ErrorMessage = "Group name is required")]
        [StringLength(100, ErrorMessage = "Group name cannot exceed 100 characters")]
        [Display(Name = "Group Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Group type is required")]
        [Display(Name = "Group Type")]
        public GroupType GroupType { get; set; } = GroupType.Savings;

        [Required(ErrorMessage = "Maximum members is required")]
        [Range(2, 50, ErrorMessage = "Maximum members must be between 2 and 50")]
        [Display(Name = "Maximum Members")]
        public int MaxMembers { get; set; } = 10;

        [Required(ErrorMessage = "Target amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Target amount must be greater than 0")]
        [Display(Name = "Target Amount")]
        public decimal TargetAmount { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Today.AddDays(1);

        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        [Required(ErrorMessage = "Contribution frequency is required")]
        [Display(Name = "Contribution Frequency")]
        public ContributionFrequency ContributionFrequency { get; set; } = ContributionFrequency.Monthly;        [Required(ErrorMessage = "Contribution amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Contribution amount must be greater than 0")]
        [Display(Name = "Contribution Amount")]
        public decimal ContributionAmount { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Minimum contribution must be 0 or greater")]
        [Display(Name = "Minimum Contribution")]
        public decimal MinContribution { get; set; } = 0;

        [Range(0, double.MaxValue, ErrorMessage = "Maximum contribution must be 0 or greater")]
        [Display(Name = "Maximum Contribution")]
        public decimal MaxContribution { get; set; } = 0;

        [Display(Name = "Public Group")]
        public bool IsPublic { get; set; } = true;

        [Display(Name = "Require Approval")]
        public bool RequireApproval { get; set; } = false;

        [StringLength(1000, ErrorMessage = "Rules cannot exceed 1000 characters")]
        [Display(Name = "Group Rules")]
        public string Rules { get; set; } = string.Empty;

        [Display(Name = "Group Image URL")]
        public string? GroupImage { get; set; }
    }

    public class EditThriftGroupViewModel : CreateThriftGroupViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string GroupCode { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public decimal CurrentAmount { get; set; }
        public int CurrentMemberCount { get; set; }
    }

    public class JoinGroupByCodeViewModel
    {
        [Required(ErrorMessage = "Group code is required")]
        [StringLength(10, MinimumLength = 6, ErrorMessage = "Group code must be between 6 and 10 characters")]
        [Display(Name = "Group Code")]
        public string GroupCode { get; set; } = string.Empty;
    }

    public class ContributeToGroupViewModel
    {
        public string GroupId { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public decimal ContributionAmount { get; set; }
        public decimal CurrentAmount { get; set; }
        public decimal TargetAmount { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        [Display(Name = "Contribution Amount")]
        public decimal Amount { get; set; }

        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
        [Display(Name = "Description (Optional)")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; } = "Cash";
    }

    public class GroupMemberViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public GroupRole Role { get; set; }
        public DateTime JoinedAt { get; set; }
        public decimal TotalContributions { get; set; }
        public DateTime? LastContributionDate { get; set; }
        public bool IsActive { get; set; }
        public int PayoutPosition { get; set; }
        public bool HasReceivedPayout { get; set; }
    }

    public class GroupTransactionViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string GroupId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public GroupTransactionType TransactionType { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public TransactionStatus Status { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string? ReferenceNumber { get; set; }
    }

    public class InviteMemberViewModel
    {
        public string GroupId { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Message cannot exceed 200 characters")]
        [Display(Name = "Personal Message (Optional)")]
        public string Message { get; set; } = string.Empty;
    }

    public class GroupInvitationViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string GroupId { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public string InvitedBy { get; set; } = string.Empty;
        public string InvitedByName { get; set; } = string.Empty;
        public string? InvitedEmail { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public InvitationStatus Status { get; set; }
        public string? Message { get; set; }
        public bool IsExpired => DateTime.UtcNow > ExpiresAt;
        public int DaysUntilExpiry => (int)(ExpiresAt - DateTime.UtcNow).TotalDays;
    }

    public class GroupSummaryViewModel
    {
        public int TotalGroups { get; set; }
        public int ActiveGroups { get; set; }
        public int CompletedGroups { get; set; }
        public decimal TotalContributions { get; set; }
        public decimal TotalReceived { get; set; }
        public int PendingInvitations { get; set; }
        public ThriftGroup? NextPayoutGroup { get; set; }
        public List<ThriftGroup> RecentGroups { get; set; } = new();
    }
}
