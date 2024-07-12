using Microsoft.EntityFrameworkCore.Diagnostics;
using RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions.Conditions;
using RhDev.Common.Workflow.Core.DataAccess.Sql.Repository;
using RhDev.Common.Workflow.Core.Management;
using RhDev.Common.Workflow.Events;
using RhDev.Common.Workflow.Security;
using RhDev.Common.Web.Core;
using RhDev.Common.Web.Core.Workflow;
using RhDev.Common.Web.Core.DataAccess.Workflow;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow;

namespace RhDev.Common.Workflow.Impl.Events
{
    public class WriteHistoryEventHandler : ActionEventHandlerBase, IStateTransitionEventHandler
    {
        private readonly ICentralClockProvider clockProvider;
        private readonly IConditionEvaluator conditionEvaluator;
        private readonly IWorkflowInstanceHistoryRepository workflowInstanceHistoryRepository;
        private readonly IExternalWorkflowHistoryProvider[] externalProviders;

        public WriteHistoryEventHandler(
            ICentralClockProvider clockProvider,
            IConditionEvaluator conditionEvaluator,
            IWorkflowInstanceHistoryRepository workflowInstanceHistoryRepository,
            IPropertyEvaluator propertyEvaluator,
            IWorkflowMembershipProvider workflowMembershipProvider, 
            IExternalWorkflowHistoryProvider[] externalProviders) : base(propertyEvaluator, workflowMembershipProvider)
        {
            HandlerState = new Dictionary<string, string>();
            this.clockProvider = clockProvider;
            this.conditionEvaluator = conditionEvaluator;
            this.workflowInstanceHistoryRepository = workflowInstanceHistoryRepository;
            this.externalProviders = externalProviders;
        }
        public int EvaluationOrder => 3;
        public string UserData { get; }
        public IDictionary<string, string> HandlerState { get; }

        public async Task RollbackAsync(
            object sender,
            SectionDesignation designation,
            StateManagementCommonTriggerProperties props,
            WorkflowDocumentIdentifier workflowDocumentIdentifier,
            Configuration.StateMachineConfig.Transitions.Transition transition,
            WorkflowTransitionLog transaction)
        {
        }

        public async Task OnTransitionAsync(object sender, StateTransitionEventArgs args)
        {
            var trigger = args.Transition.StateManagementTrigger;

            var writeHistory = !Equals(null, trigger?.History) && trigger.History.Count > 0;

            var now = clockProvider.Now().ExportDateTime;

            if (writeHistory)
            {
                foreach (var historyEntry in trigger!.History)
                {
                    if (historyEntry.Condition is not null )
                    {
                        var precondition = await HistoryPrecondition(historyEntry.Condition, args);

                        if (!precondition) continue;
                    }
                                        
                    args.CheckValidWorkflowId();
                                        
                    var userId = args.Parameters.UserRespondedId;

                    var entry = !Equals(null, historyEntry?.Entry.Operand) ?
                        (await _propertyEvaluator.EvaluateAsync(historyEntry.Entry.Operand, args))?.ToString() :
                        string.Empty;

                    var message = !Equals(null, historyEntry?.Message?.Operand) ?
                        (await _propertyEvaluator.EvaluateAsync(historyEntry.Message.Operand, args))?.ToString() :
                        string.Empty;

                    if (string.IsNullOrWhiteSpace(entry) && string.IsNullOrWhiteSpace(message)) return;

                    var history = new WorkflowInstanceHistory
                    {
                        WorkflowInstanceId = args.WorkflowId,
                        Date = now,
                        Event = entry,
                        Message = message,
                        UserId = userId
                    };

                    await workflowInstanceHistoryRepository.CreateAsync(history);

                    foreach (var externalProvider in externalProviders)
                    {
                        await externalProvider.WriteAsync(args.WorkflowDocumentIdentifier, now, entry, message, userId);
                    }
                }
            }
        }

        async Task<bool> HistoryPrecondition(ConditionExpression condition, StateTransitionEventArgs args) => await conditionEvaluator.EvaluateAsync(args, condition);
    }
}
