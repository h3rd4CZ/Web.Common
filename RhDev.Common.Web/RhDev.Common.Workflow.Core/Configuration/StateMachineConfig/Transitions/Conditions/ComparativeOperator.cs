namespace RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions.Conditions
{
    [Flags]
    public enum ComparativeOperator
    {
        Unknown = 0,
        Equals = 1,
        Less = 1 << 1,
        Greater = 1 << 2,
        Empty = 1 << 3,
        NotEmpty = 1 << 4,
        NotEquals = 1 << 5,
        LessOrEqual = 1 << 6,
        GreaterOrEqual = 1 << 7,
        IsGroupEmpty = 1 << 8,
        IsGroupNotEmpty = 1 << 9,
        UserInGroup = 1 << 10,
        CurrentUserInGroup = 1 << 11,
        DBDocumentExist = 1 << 12,
        CurrentUserIsSystem = 1 << 13,
        IsNull = 1 << 14,
        IsNotNull = 1 << 15,
        Contains = 1 << 16,
        NotContains = 1 << 17,
        AggregatedApproval = 1 << 18
    }
}
