using Microsoft.AspNetCore.Identity;
using RhDev.Common.Web.Core.Composition;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow;
using RhDev.Common.Web.Core.Workflow;
using RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions;
using RhDev.Common.Workflow.Events;
using RhDev.Common.Workflow.Management;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RhDev.Common.Workflow
{
    public interface IConfigurableStateMachine<in TTRigger, in TParametrizedTrigger> : IAutoregisteredService
    {
        SectionDesignation Designation { get; }
        WorkflowInstance WorkflowInstance { get; }
        StateManagementCommonTriggerProperties UserData { get; }
        StateTransitionEventArgs StateTransitionEventArgs { get; }
        Task<IList<Transition>> GetAllPermittedTransitionsAsync();
        Task FireParametrizedAsync(TTRigger trigger, TParametrizedTrigger props);
        bool IsInEndState { get; }
        Task RollbackTransactionAsync(string triggerCode);
        Task<TaskRespondStatus> CompleteTaskAsync();
    }
}
