using RhDev.Common.Workflow.DataAccess;
using RhDev.Common.Workflow.Events;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Stores;
using Microsoft.EntityFrameworkCore;
using RhDev.Common.Web.Core.Utils;
using RhDev.Common.Web.Core.Workflow;


namespace RhDev.Common.Workflow.Impl.Transition
{
    public class TransitionEvaluator : TransitionEvaluatorBase, IStateTransitionEvaluator
    {
        private readonly IConditionEvaluator _conditionEvaluator;

        public TransitionEvaluator(
            IConditionEvaluator conditionEvaluator,
            IStateManagementDocumentDataRepositoryFacade stateManagementDocumentDataRepositoryFacade,
            IDynamicDataStoreRepository<DbContext> dynamicDataStoreRepository) :
            base(stateManagementDocumentDataRepositoryFacade, dynamicDataStoreRepository)
        {
            _conditionEvaluator = conditionEvaluator;
        }

        public async Task<bool> EvaluateTransitionConditionAsync(bool evaluatePermission, StateTransitionEventArgs args)
        {
            Guard.NotNull(args, nameof(args));

            if (StateManagementConditionBypass.IsActive.HasValue && StateManagementConditionBypass.IsActive.Value) return true;

            var userId = args.UserId;

            if (evaluatePermission && string.IsNullOrWhiteSpace(userId)) throw new ArgumentNullException(nameof(userId));

            return await EvaluateTemplate(evaluatePermission, args)
                .ConfigureAwait(false);
        }
                
        protected async override Task<bool> EvaluateConditions(StateTransitionEventArgs args)
        {
            return await _conditionEvaluator.EvaluateAsync(args, args.Transition?.Condition);
        }

        public void EvaluateUserProperties(Configuration.StateMachineConfig.Transitions.Transition transition, StateManagementCommonTriggerProperties userData)
        {
            EvaluateUserData(transition, userData);
        }
    }

}
