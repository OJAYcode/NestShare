namespace Thrift.Models
{
    public enum GroupType
    {
        Savings = 0,
        RotatingSavings = 1,
        Investment = 2,
        Loan = 3,
        Emergency = 4,
        General = 5
    }

    public enum GroupRole
    {
        Member = 0,
        Admin = 1,
        Moderator = 2,
        Creator = 3
    }

    public enum ContributionFrequency
    {
        Daily = 0,
        Weekly = 1,
        Monthly = 2,
        Quarterly = 3,
        Yearly = 4,
        OneTime = 5
    }

    public enum GroupTransactionType
    {
        Contribution = 0,
        Withdrawal = 1,
        Transfer = 2,
        Payout = 3,
        Fee = 4,
        Penalty = 5,
        Interest = 6,
        Bonus = 7
    }

    public enum TransactionStatus
    {
        Pending = 0,
        Completed = 1,
        Failed = 2,
        Cancelled = 3,
        Processing = 4
    }

    public enum InvitationStatus
    {
        Pending = 0,
        Accepted = 1,
        Declined = 2,
        Expired = 3,
        Cancelled = 4
    }
}
