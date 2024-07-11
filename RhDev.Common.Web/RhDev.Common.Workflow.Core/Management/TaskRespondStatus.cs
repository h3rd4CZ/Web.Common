namespace RhDev.Common.Workflow.Management
{
    public enum TaskRespondStatus
    {
        NoTasksAssigned = 0,
        WaitToOthers = 1,
        Fire = 1 << 1
    }
}
