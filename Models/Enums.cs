namespace Thrift.Models
{
    public enum SavingsStatus
    {
        Active,
        Paused,
        Completed,
        Cancelled
    }

    public enum Priority
    {
        Low,
        Medium,
        High,
        Urgent
    }

    public enum TransactionType
    {
        Deposit,
        Withdrawal,
        Adjustment
    }

    public enum SavingsCategory
    {
        Emergency,
        Retirement,
        Education,
        Travel,
        Home,
        Vehicle,
        Wedding,
        Electronics,
        Investment,
        Debt,
        Other
    }
}