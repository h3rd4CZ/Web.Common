using RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions;
using System.Collections.Generic;
using RhDev.Common.Web.Core.Composition;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow;
using RhDev.Common.Web.Core.Workflow;
using RhDev.Common.Web.Core.DataAccess.Workflow;

namespace RhDev.Common.Workflow.Events
{
    public interface IStateTransitionEventHandler : IAutoregisteredService
    {
        int EvaluationOrder { get; }
        void RehydrateState(Dictionary<string, object> dic);
        Dictionary<string, object> HandlerState { get; }
        Task RollbackAsync(
            object sender,
            SectionDesignation designation,
            StateManagementCommonTriggerProperties props,
            WorkflowDocumentIdentifier workflowDocumentIdentifier,
            Transition transactionTransition,
            WorkflowTransitionLog transaction);
        Task OnTransitionAsync(object sender, StateTransitionEventArgs args);
    }
}
