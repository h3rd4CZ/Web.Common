using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow;
using RhDev.Common.Workflow.Core.DataAccess.Sql.Repository;
using RhDev.Common.Workflow.Events;
using RhDev.Common.Workflow.Impl.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RhDev.Common.Workflow.Impl.Logging
{
    public class StateTransitionExternalLogging : IStateTransitionExternalLogging
    {
        private readonly IWorkflowDocumentRepository workflowDocumentRepository;
        private readonly IWorkflowTransitionLogRepository workflowTransitionLogRepository;

        public StateTransitionExternalLogging(
            IWorkflowDocumentRepository workflowDocumentRepository,
            IWorkflowTransitionLogRepository workflowTransitionLogRepository)
        {
            this.workflowDocumentRepository = workflowDocumentRepository;
            this.workflowTransitionLogRepository = workflowTransitionLogRepository;
        }

        public async Task LogAsync(string handlerClass, Exception exception, StateTransitionEventArgs stateArgs, DateTime startEvent, DateTime endEvent, Dictionary<string, object> handlerState)
        {            
            var parentWorkflowId = stateArgs.WorkflowId;

            if (parentWorkflowId == default) return;

            var newLog = new WorkflowTransitionLog()
            {
                BeginDate = startEvent,
                EndDate = endEvent,
                FromState = stateArgs.SourceState,
                ToState = stateArgs.DestinationState,
                Handler = handlerClass,
                Result = !Equals(null, exception) ? exception.ToString() : string.Empty,
                TransitionId = stateArgs.Parameters.TransitionId.ToString(),
                UserId = stateArgs.Parameters.UserRespondedId,
                UserData = handlerState,
                WorkflowInstanceId = parentWorkflowId,

            };

            await workflowTransitionLogRepository.CreateAsync(newLog);
        }

        private string TrimResult(string result)
        {
            if (string.IsNullOrEmpty(result)) return result;

            return result.Substring(0, Math.Min(result.Length, 4000));
        }
    }
}
