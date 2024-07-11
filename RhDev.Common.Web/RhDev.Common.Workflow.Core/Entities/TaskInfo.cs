using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow;
using System;

namespace RhDev.Common.Workflow.Entities
{
    [Serializable]
    public class TaskInfo
    {
        public DocumentInfo ParentDocument { get; set; }
        public string Title { get; set; }
        public DateTime AssignedOn { get; set; }
        public string? ResolvedBy { get; set; }
        public DateTime? ResolvedOn { get; set; }
        public string StateId { get; set; }
        public WorkflowDatabaseTaskStatus Status { get; set; }
        public int Id { get; set; }
        public string AssignedTo { get; set; }
        public TaskAssigneeType AssigneeType { get; set; }
        public DateTime? DueDate { get; set; }
        public string UserData { get; set; }

        public static TaskInfo FillFrom(WorkflowTask task)
        {
            return new TaskInfo()
            {
                Title = task.Title,
                AssignedOn = task.AssignedOn,
                Status = task.Status,
                Id = task.Id,
                ResolvedOn = task.ResolvedOn,
                UserData = task.UserData,
                AssignedTo = task.AssignedTo,
                AssigneeType = task.AssigneeType,
                DueDate = task.DueDate,
                ResolvedBy = task.ResolvedById,
            };
        }

        public static WorkflowTask FillFrom(TaskInfo taskInfo)
        {
            if (taskInfo == null) throw new ArgumentNullException(nameof(taskInfo));

            return new WorkflowTask()
            {
                Title = taskInfo.Title,
                AssignedOn = taskInfo.AssignedOn,
                Id = taskInfo.Id,
                ResolvedOn = taskInfo.ResolvedOn,
                ResolvedById = taskInfo.ResolvedBy,
                Status = taskInfo.Status,
                AssignedTo = taskInfo.AssignedTo,
                AssigneeType = taskInfo.AssigneeType,
                DueDate = taskInfo.DueDate,
                UserData = taskInfo.UserData,
            };
        }
    }
}
