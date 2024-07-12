using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RhDev.Common.Web.Core;
using RhDev.Common.Web.Core.DataAccess.Workflow;
using RhDev.Common.Web.Core.Test;
using RhDev.Common.Web.Core.Workflow;
using RhDev.Common.Workflow.Configuration;
using RhDev.Common.Workflow.Core.DataAccess.Sql.Repository;
using RhDev.Common.Workflow.Core.Security;
using RhDev.Common.Workflow.Core.Test.Data;
using RhDev.Common.Workflow.Impl.Utils;
using RhDev.Common.Workflow.Monitor;
using System.Reflection;
using Xunit.Abstractions;

namespace RhDev.Common.Workflow.Core.Test
{
    public class WfPassTest : IntegrationTestBase
    {

        public WfPassTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

        [Fact]
        public async Task WfPassTest_Base()
        {
            var services = TestProlog(true, nameof(WfPassTest));

            var planner = services.GetRequiredService<IWorkflowRunnerPlanner>();

            var dataAccessor = services.GetRequiredService<IRawEntityDataAccessor>();
            var wfService = services.GetRequiredService<IWorkflowService>();
            var fileData = File.ReadAllBytes("Assets\\machine.json");
            var fileDef = new WorkflowDefinitionFile { Data = fileData, Name = "machine.json", Version = "1.0" };

            var instanceRepo = services.GetRequiredService<IWorkflowInstanceRepository>();
            var historyRepo = services.GetRequiredService<IWorkflowInstanceHistoryRepository>();
            var taskRepo = services.GetRequiredService<IWorkflowTaskRepository>();
            var requestsRepo = services.GetRequiredService<IWorkflowTransitionRequestRepository>();
            var metadataRepo = services.GetRequiredService<ITestWorkflowMetadataRepository>();

            var identifier = new WorkflowDocumentIdentifier(
                SectionDesignation.Empty,
                    new WorkflowDocumentIdentificator(1, typeof(TestWorkflowMetadata).AssemblyQualifiedName!), "2024-1", 1);

            var wf = await wfService.StartWorkflowAsync(identifier, fileDef, SystemUserNames.System.ToString(), new List<Workflow.Management.WorkflowStartProperty>
            {
                 new Workflow.Management.WorkflowStartProperty{ Name = "SuperValue", Type = WorkflowDataType.Text, Value = "svv"}
            });


            //1st system transition
            var rp = new WorkflowRuntimeParametersBuilder()
                .WithDocumentIdentifier(identifier)
                .IsSystem(true)
                .WithUserId(SystemUserNames.System.ToString())
                .WithWorkflowInfo(wf);

            var transitions = await wfService.GetAllPermittedTransitionsAsync(rp.Build());

            var transition = transitions.FirstOrDefault().TransitionInfos[0];
            var trigger = transition.Trigger;
            rp.WithUserData(StateManagementCommonTriggerProperties.DataWithUserResponded(SystemUserNames.System.ToString(),
                new List<WorkflowTriggerParameter>()
                {
                    new WorkflowTriggerParameter{ Name = "Datum", Value=DateTime.Now.ToString()}
                }, trigger.Code));
            await wfService.EnqueueTransitionRequestAsync(WorkflowTransitionRequestPayload.Create(rp.Build(), StateTransitionSources.SYSTEM), false);

            var tasks = await taskRepo.ReadAllAsync();
            var history = await historyRepo.ReadAllAsync();
            var requests = await requestsRepo.ReadAllAsync();
            var metadata = await metadataRepo.ReadByIdAsync(1);


            //2nd complete the task
            var rpTask = new WorkflowRuntimeParametersBuilder()
                .WithDocumentIdentifier(identifier)
                .WithUserId(SystemUserNames.System.ToString())
                .WithWorkflowInfo(wf);

            var taskTransitions = await wfService.GetAllPermittedTransitionsAsync(rpTask.Build());

            var taskTransition = taskTransitions.FirstOrDefault().TransitionInfos[0];
            var taskTrigger = taskTransition.Trigger;
            rpTask.WithUserData(StateManagementCommonTriggerProperties.DataWithUserResponded(SystemUserNames.System.ToString(),
                new List<WorkflowTriggerParameter>()
                {
                    new WorkflowTriggerParameter{ Name = "Approver", Value=SystemUserNames.System.ToString()}
                }, taskTrigger.Code));

            await wfService.CompleteTaskAsync(rpTask.Build());

            Thread.Sleep(10000);

            tasks = await taskRepo.ReadAllAsync();
            history = await historyRepo.ReadAllAsync();
            requests = await requestsRepo.ReadAllAsync();
            metadata = await metadataRepo.ReadByIdAsync(1);

        }


        IServiceProvider TestProlog(bool seed = false, string databaseName = default!)
        {
            RegisterEfInMemoryMock(seed, databaseName);

            var host = Host<WorkflowDatabaseTestContext>(
            new[]
            {
                RhDev.Common.Web.Core.Composition.CompositionDefinition.GetDefinition(),
                Composition.CompositionDefinition.GetDefinition()
            }, useDbContextFactory: false,

            serviceConfiguration: (ctx, services) =>
            {
                services.AddScoped<IWorkflowGroupMembershipResolver, TestWorkflowGroupMembershipResolver>();

                services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<WorkflowDatabaseTestContext>()
                .AddDefaultTokenProviders();

            });

            return host.Services;
        }

        void RegisterEfInMemoryMock(bool seed, string? databaseName = default)
        {
            if (seed)
            {
                RegisterEfInMemoryMock<WorkflowDatabaseTestContext, DbContext>(ctx =>
                {
                    SeedDatabase(ctx);

                    ctx.SaveChanges();

                }, databaseName);
            }
            else
            {
                RegisterEfInMemoryMock<WorkflowDatabaseTestContext, DbContext>(ctx =>
                {
                }, databaseName);
            }
        }

        void SeedDatabase(WorkflowDatabaseTestContext ctx)
        {
            ctx.AddRange(new List<IdentityRole>
                    {
                        new IdentityRole{  Id = "120CC36E-8346-4AAF-B3BA-AD6B44425AC7", Name = "Dms" }
                    });
                        

            ctx.AddRange(new List<IdentityUser>
            {
                new IdentityUser
                {
                     Id = SystemUserNames.System.ToString(),
                      UserName = "System",
                      Email = "system@system.com"
                }
            });

            ctx.AddRange(new List<IdentityUserRole<string>>
            {
                new IdentityUserRole<string>
                {
                     RoleId = "120CC36E-8346-4AAF-B3BA-AD6B44425AC7",
                     UserId = SystemUserNames.System.ToString()
                }
            });

            ctx.AddRange(new List<TestWorkflowMetadata>
                    {
                        new TestWorkflowMetadata
                        {
                            WorkflowAssignedTo = SystemUserNames.System.ToString(),
                            InvoiceNumber = "abcd",
                            Id = 1,
                            DocumentNumber = "2024-888",
                            State = "Initial",
                            MinedSuccessfully = false,
                            //Created= DateTime.Now
                        }
                    });
        }
    }
}