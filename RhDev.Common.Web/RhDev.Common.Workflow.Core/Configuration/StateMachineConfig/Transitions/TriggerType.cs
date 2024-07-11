using System;

namespace RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions
{
    [Flags]
    public enum TriggerType
    {
        Unknown = 0,
        Approval = 1,
        Rejection = 1 << 1,
        Aggregate = 1 << 2,
        NotRequired = 1 << 3,
        Received = 1 << 4,
        Storno = 1 << 5,
        ApprovalRequest = 1 << 6,
        ReturnedToProcessor = 1 << 7,
        SubmitToEditor = 1 << 8,
        Repair = 1 << 9,
        ToAccountant = 1 << 10,
        Assign = 1 << 11,
        ToCompletation = 1 << 12,
        Accounted = 1 << 13,
        Completed = 1 << 14,
        Accepted = 1 << 15,
        TakeNote = 1 << 16,
        Created = 1 << 17,
        ForChecking = 1 << 18,
        Repaired = 1 << 19,
        HandOver = 1 << 20,
        Taken = 1 << 21,
        ReturnedToPostRoom = 1 << 22,
        ChangeEditor = 1 << 23,
        ToRedo = 1 << 24,
        ApprovalByScannedAttachment = 1 << 25
    }
}
