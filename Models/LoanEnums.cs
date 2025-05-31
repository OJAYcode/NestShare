namespace Thrift.Models
{
    public enum LoanType
    {
        Mortgage,
        CarLoan,
        StudentLoan,
        PersonalLoan,
        CreditCard,
        BusinessLoan,
        HELOCLoan,
        Other
    }    public enum LoanStatus
    {
        Active,
        PaidOff,
        Delinquent,
        InDefault,
        Closed,
        Refinanced,
        OnHold,
        Completed,
        Defaulted
    }

    public enum InterestType
    {
        Fixed,
        Variable,
        Hybrid
    }    public enum PaymentFrequency
    {
        Weekly,
        Biweekly,
        Monthly,
        Quarterly,
        SemiAnnually,
        Annually
    }

    public enum LoanCalculationType
    {
        StandardAmortization,
        InterestOnly,
        BalloonPayment,
        FixedPrincipal,
        Custom
    }    public enum PaymentStatus
    {
        Scheduled,
        Paid,
        Partial,
        Late,
        Missed,
        Future,
        Pending,
        Completed,
        Failed
    }
}