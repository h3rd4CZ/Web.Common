using Microsoft.AspNetCore.Identity;
using RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions;
using RhDev.Common.Workflow.DataAccess;
using RhDev.Common.Workflow.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using System.Reflection.Metadata.Ecma335;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Stores;
using Microsoft.EntityFrameworkCore;
using RhDev.Common.Web.Core.Utils;
using RhDev.Common.Web.Core.Workflow;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow;

namespace RhDev.Common.Workflow.Impl.Transition
{
    public abstract class TransitionEvaluatorBase
    {
        private readonly IStateManagementDocumentDataRepositoryFacade stateManagementDocumentDataRepositoryFacade;
        private readonly IDynamicDataStoreRepository<DbContext> dynamicDataStoreRepository;

        protected TransitionEvaluatorBase(
            IStateManagementDocumentDataRepositoryFacade stateManagementDocumentDataRepositoryFacade,
            IDynamicDataStoreRepository<DbContext> dynamicDataStoreRepository)
        {
            this.stateManagementDocumentDataRepositoryFacade = stateManagementDocumentDataRepositoryFacade;
            this.dynamicDataStoreRepository = dynamicDataStoreRepository;
        }

        protected abstract Task<bool> EvaluateConditions(StateTransitionEventArgs args);
        
        protected async Task<bool> EvaluateTemplate(bool evaluatePermission, StateTransitionEventArgs args)
        {                                    
            Guard.NotNull(args, nameof(args));

            EvaluateUserData(args.Transition, args.Parameters);

            args.CheckValidWorkflowId();

            var permission = evaluatePermission
                ? await Permission(args.UserId, args.UserGroups, args.WorkflowId)
                : true;

            return permission && await EvaluateConditions(args).ConfigureAwait(false);
        }
             
        private async Task<bool> Permission(string userId, IList<string> userGroups, int workflowId)
        {
            var user = await dynamicDataStoreRepository.ReadEntityByLambdaAsync<IdentityUser>(typeof(IdentityUser), u => u.Id == userId);
                                    
            Guard.NotNull(user);

            var tasks =
                await stateManagementDocumentDataRepositoryFacade
                .GetAllUserAssignedTasksAsync(
                    workflowId,
                    user.Id,
                    userGroups?.ToArray(),
                    WorkflowDatabaseTaskStatus.NotStarted);

            return tasks.Count > 0;
        }

        protected void EvaluateUserData(Configuration.StateMachineConfig.Transitions.Transition transition, StateManagementCommonTriggerProperties userData)
        {
            Guard.NotNull(transition, nameof(transition));

            if (Equals(null, userData)) return;

            if (!userData.ValidUserTransitionData) return;

            if (Equals(null, transition.StateManagementTrigger.Parameters) || transition.StateManagementTrigger.Parameters.Count == 0) return;

            CheckForRequiredProperty(transition.StateManagementTrigger.Parameters, userData);
        }

        private void CheckForRequiredProperty(IList<TransitionParameter> transitionParameters, StateManagementCommonTriggerProperties userData)
        {
            Guard.NotNull(transitionParameters, nameof(transitionParameters));

            var allRequired = transitionParameters.Where(p => p.Required);

            if (!allRequired.Any()) return;

            var allRequiredMissing = allRequired.Where(r => !userData.TriggerParameters.Any(t => !Equals(null, t.Name) && t.Name.Equals(r.PropertyName, StringComparison.OrdinalIgnoreCase)));

            if (allRequiredMissing.Any())
                throw new InvalidOperationException(
                    $"Some of required parameters are not present, please make sure the mandatory variables has been set. These parameters are not present : {string.Join(",", allRequiredMissing.Select(m => m.PropertyName))}");
        }
    }

}
