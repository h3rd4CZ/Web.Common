using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow;
using RhDev.Common.Web.Core.DataAccess.Workflow;
using RhDev.Common.Web.Core.Utils;
using RhDev.Common.Web.Core.Workflow;
using RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions;
using RhDev.Common.Workflow.Core.DataAccess.Sql.Repository;
using RhDev.Common.Workflow.Entities;
using RhDev.Common.Workflow.Events;
using RhDev.Common.Workflow.Security;
using System;
using System.Collections.Generic;
using System.Text;

namespace RhDev.Common.Workflow.Impl.Events
{
    public class UpdateDocumentMetadataEventHandler : ActionEventHandlerBase, IStateTransitionEventHandler
    {
        private const string STATE_KEY_DOCUMENTID = "DocumentId";
        private const string STATE_KEY_ORIGINALSTATE = "OriginalState";
        private const string STATE_KEY_ISNEW = "IsNew";
        private readonly IWorkflowInstanceRepository workflowInstanceRepository;

        public UpdateDocumentMetadataEventHandler(
            IWorkflowInstanceRepository workflowInstanceRepository,
            IPropertyEvaluator propertyEvaluator,
            IWorkflowMembershipProvider workflowMembershipProvider
            ) : base(propertyEvaluator, workflowMembershipProvider)
        {
            HandlerState = new Dictionary<string, string>();
            this.workflowInstanceRepository = workflowInstanceRepository;
        }
        public int EvaluationOrder => 2;
        public string UserData { get; }
        public IDictionary<string, string> HandlerState { get; }

        public async System.Threading.Tasks.Task RollbackAsync(
            object sender,
            SectionDesignation designation,
            StateManagementCommonTriggerProperties props,
            WorkflowDocumentIdentifier workflowDocumentIdentifier,
            Configuration.StateMachineConfig.Transitions.Transition transition,
            WorkflowTransitionLog transaction)
        {
            //TODO what about rollback
            //var data = DeserializeParams(transaction.Data);
            //string docId = GetParam(data, STATE_KEY_DOCUMENTID, false);
            //string originalState = GetParam(data, STATE_KEY_ORIGINALSTATE, false);
            //string isNew = GetParam(data, STATE_KEY_ISNEW, false);

            //var documents = dataStoreAcessRepositoryFactory.GetStoreRepository<Document>();

            //if (!string.IsNullOrEmpty(isNew))
            //{
            //    if (string.IsNullOrEmpty(docId)) throw new InvalidOperationException($"DocId param is empty");

            //    var docIdInt = Convert.ToInt32(docId);

            //    try
            //    {
            //        var dbDocument = documents.ReadById(docIdInt);
            //        documents.Delete((int)dbDocument.Id);
            //    }
            //    catch (EntityNotFoundException) { }
            //}
            //else
            //{
            //    if (string.IsNullOrEmpty(docId)) return;


            //    var doc = documents.ReadById(Convert.ToInt32(docId));
            //    //TODO set state on wf instance
            //    //doc.State = originalState;

            //    documents.Update(doc);
            //}
        }

        public async Task OnTransitionAsync(object sender, StateTransitionEventArgs args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));

            var stateMachine = sender as ConfigurableStateMachineBase;

            if (Equals(null, stateMachine))
                throw new InvalidOperationException($"Sender is not a ConfigurableStateMachineBase");

            args.CheckValidWorkflowId();

            var workflowInstanceId = args.WorkflowId;

            var workflowInstance = await workflowInstanceRepository.ReadByIdAsync(workflowInstanceId);

            var toStateDef = stateMachine.GetStateDefinitionByTitle(args.DestinationState);
            var isToStateSystem = stateMachine.IsStateSystem(args.DestinationState);

            Guard.NotNull(toStateDef);

            var isFinalState = toStateDef.IsEnd;

            workflowInstance.WorkflowStateSystem = isToStateSystem;

            if (isFinalState) workflowInstance.Finished = DateTime.Now;

            await workflowInstanceRepository.UpdateAsync(workflowInstance);
        }
    }
}
