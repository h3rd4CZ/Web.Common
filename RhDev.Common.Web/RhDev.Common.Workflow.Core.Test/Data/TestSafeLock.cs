using Medallion.Threading.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RhDev.Common.Web.Core.Configuration;
using RhDev.Common.Web.Core.DataAccess.Workflow;
using RhDev.Common.Web.Core.Utils;
using RhDev.Common.Workflow.Management;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhDev.Common.Workflow.Core.Test.Data
{
    public class TestSafeLock : ISafeLock
    {
        private readonly ILogger<TestSafeLock> logger;

        public TestSafeLock(

            ILogger<TestSafeLock> logger)
        {
            this.logger = logger;
        }

        public async Task UseDistributedLockForWorkflowDocumentAsync(StateMachineRuntimeParameters runtimeParameters, Func<Task> actionUnderLock, string methodName)
        {
            ValidateRuntimeParametersForDocumentLocking(runtimeParameters);

            await actionUnderLock();

        }

        public async Task<TReturn> UseDistributedLockForWorkflowDocumentAndReturnAsync<TReturn>(StateMachineRuntimeParameters runtimeParameters, Func<Task<TReturn>> funcUnderLock, string methodName)
        {
            ValidateRuntimeParametersForDocumentLocking(runtimeParameters);

            var result = await funcUnderLock();

            return result;
        }
                
        private void ValidateRuntimeParametersForDocumentLocking(StateMachineRuntimeParameters runtimeParameters)
        {
            Guard.NotNull(runtimeParameters, nameof(runtimeParameters));
            Guard.NotNull(runtimeParameters.DocumentMetadataIdentifier, nameof(runtimeParameters.DocumentMetadataIdentifier), $"There are no valid document metadata for distributed lock pattern");
        }
    }
}