using RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions;
using RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions.Conditions;
using RhDev.Common.Workflow.Events;
using RhDev.Common.Workflow.PropertyModel.Properties;
using RhDev.Common.Workflow.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using RhDev.Common.Web.Core.Utils;
using RhDev.Common.Web.Core.Workflow;

namespace RhDev.Common.Workflow.Impl.Events
{
    public abstract class ActionEventHandlerBase
    {
        protected readonly IPropertyEvaluator _propertyEvaluator;
        protected readonly IWorkflowMembershipProvider workflowMembershipProvider;

        public Dictionary<string, object> HandlerState { get; private set; } = new();
        public void RehydrateState(Dictionary<string, object> dic)
        {
            Guard.NotNull(dic);
            HandlerState = dic;
        }
        protected void SetStateParam(string key, object value) => HandlerState[key] = value;
        protected T GetParam<T>(string key, out bool exist, bool throwIfNotExist = false)
        {    
            if(HandlerState.TryGetValue(key, out object value))
            {
                exist = true;
                return PolymorphicBaseConverter.TryParseValue<T>(value);
            }
            else
            {
                if (throwIfNotExist) throw new InvalidOperationException($"No param : {key} was found in state");
                exist = false;
                return default!;
            }
        }
                
        protected ActionEventHandlerBase(
            IPropertyEvaluator propertyEvaluator,
            IWorkflowMembershipProvider workflowMembershipProvider)
        {
            this._propertyEvaluator = propertyEvaluator;
            this.workflowMembershipProvider = workflowMembershipProvider;
        }
                
        protected async Task<IList<(StateManagementUserValue assignee, List<UserInfo> extractedUsers)>> GetAssigneesFor(Operand member, bool extractGroups, StateTransitionEventArgs args, bool forceExtractGroups)
        {
            Guard.NotNull(member, nameof(member));
            Guard.NotNull(args, nameof(args));

            var userValue = await _propertyEvaluator.EvaluateAsync(member, args);

            var usersToProcess = new List<StateManagementUserValue>();

            if (userValue is StateManagementNullValue nullUser) return null!;
            else if (userValue is StateManagementUserValue user)
            {
                usersToProcess.Add(user);
            }
            else if (userValue is StateManagementArrayValue userArray && userArray.ItemType == typeof(StateManagementUserValue).Name)
            {
                if (Equals(null, userArray.Data) || userArray.Data.Count == 0) return null!;

                foreach (var userInArray in userArray.Data) usersToProcess.Add(userInArray as StateManagementUserValue);
            }
            else throw new InvalidOperationException("Task assignee operand must be on of the folowing : UserValue, Array of UserValue, Null");

            args.CheckValidWorkflowId();

            var result = new List<(StateManagementUserValue assignee, List<UserInfo> extractedUsers)> { };

            foreach (var userToProcess in usersToProcess)
            {
                var isPermissionGroup = userToProcess.IsPermissionGroup;
                
                if (!isPermissionGroup)
                {
                    result.AddRange(new List<(StateManagementUserValue, List<UserInfo>)> { (userToProcess, new List<UserInfo> { }) });
                }
                else
                {
                    var extractedUsers = extractGroups || forceExtractGroups ? await workflowMembershipProvider.ExtractGroupMembersAsync(userToProcess, args.Designation) : new List<UserInfo> { };

                    if (extractGroups)
                    {
                        result.AddRange(extractedUsers.Select(u => (new StateManagementUserValue(u, args.Designation), new List<UserInfo> { })).ToList());
                    }
                    else
                    {
                        result.AddRange(new List<(StateManagementUserValue, List<UserInfo>)> { (userToProcess, extractedUsers.ToList()) });
                    }
                }
            }

            return result;
        }
    }
}
