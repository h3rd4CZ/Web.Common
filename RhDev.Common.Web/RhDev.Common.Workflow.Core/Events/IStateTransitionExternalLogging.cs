using RhDev.Common.Web.Core.Composition;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RhDev.Common.Workflow.Events
{
    public interface IStateTransitionExternalLogging : IAutoregisteredService
    {
        Task LogAsync(string handlerClass, Exception exception, StateTransitionEventArgs stateArgs, DateTime eventStart,
            DateTime eventEnd, Dictionary<string, object> transactionData);
    }
}
