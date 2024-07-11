using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow;
using System;
using System.Collections.Generic;
using System.Text;

namespace RhDev.Common.Workflow.Entities
{
    public class TransitionLogInfo
    {
        public string TransitionId { get; set; }
        public string? User { get; set; }
        public string Section { get; set; }
        public string FromState { get; set; }
        public string ToState { get; set; }
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Result { get; set; }
        public string Handler { get; set; }
        public Dictionary<string, object> UserData { get; set; }
        public static TransitionLogInfo FillFrom(WorkflowTransitionLog transitionLog)
        {
            return new TransitionLogInfo()
            {
                BeginDate = transitionLog.BeginDate ?? DateTime.MinValue,
                EndDate = transitionLog.EndDate ?? DateTime.MinValue,
                FromState = transitionLog.FromState,
                Handler = transitionLog.Handler,
                Result = transitionLog.Result,
                ToState = transitionLog.ToState,
                TransitionId = transitionLog.TransitionId,
                User = transitionLog.UserId,
                UserData = transitionLog.UserData
            };
        }
    }
}
