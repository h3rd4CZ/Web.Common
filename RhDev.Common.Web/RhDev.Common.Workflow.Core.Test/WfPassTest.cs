using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using RhDev.Common.Web.Core;
using RhDev.Common.Web.Core.DataAccess.Workflow;
using RhDev.Common.Web.Core.Test;
using RhDev.Common.Web.Core.Workflow;
using RhDev.Common.Workflow.Configuration;
using RhDev.Common.Workflow.Core.DataAccess.Sql.Repository;
using RhDev.Common.Workflow.Core.Security;
using RhDev.Common.Workflow.Core.Test.Data;
using RhDev.Common.Workflow.Impl.Utils;
using RhDev.Common.Workflow.Management;
using RhDev.Common.Workflow.Monitor;
using System;
using System.Data;
using System.Reflection;
using Xunit.Abstractions;

namespace RhDev.Common.Workflow.Core.Test
{
    public class WfPassTest : IntegrationTestBase
    {
        private const string EditorRoleId = "120CC36E-8346-4AAF-B3BA-AD6B44425AC7";
        private const string ApproverRoleId = "E16E57D3-CB81-4EC4-9B1A-DB8B4E7C9D5D";
        private const string AccountantRoleId = "061CEC25-DEDC-4F54-9DB5-0E69B971EB9D";

        private const string EditorUserId = "951C2574-50A8-4D51-9960-2C9D175BDF5C";
        private const string ApproverUserId = "F1E9F32F-6240-4C88-93D3-A6929439CD3D";
        private const string AccountantUserId = "F1F67122-CAAA-4E15-B84C-A861E9972425";

        public WfPassTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

        [Fact]
        public async Task WfPassTest_Base()
        {
            var services = TestProlog(true, nameof(WfPassTest));

            var wfService = services.GetRequiredService<IWorkflowService>();
            var fileData = File.ReadAllBytes("Assets\\machine.json");
            var fileDef = new WorkflowDefinitionFile { Data = fileData, Name = "machine.json", Version = "1.0" };

            var instanceRepo = services.GetRequiredService<IWorkflowInstanceRepository>();
            var historyRepo = services.GetRequiredService<IWorkflowInstanceHistoryRepository>();
            var taskRepo = services.GetRequiredService<IWorkflowTaskRepository>();
            var requestsRepo = services.GetRequiredService<IWorkflowTransitionRequestRepository>();
            var metadataRepo = services.GetRequiredService<ITestWorkflowMetadataRepository>();
            var groupMembershipResolver = services.GetRequiredService<IWorkflowGroupMembershipResolver>();
            var serviceProvider = services.GetRequiredService<IServiceProvider>();

            var um = services.GetRequiredService<UserManager<IdentityUser>>();

            var roles = await um.GetUsersInRoleAsync("Editor");

            var identifier = new WorkflowDocumentIdentifier(
                SectionDesignation.Empty,
                    new WorkflowDocumentIdentificator(1, typeof(TestWorkflowMetadata).AssemblyQualifiedName!), "2024-001", 1);

            var wf = await wfService.StartWorkflowAsync(identifier, fileDef, SystemUserNames.System.ToString(), new List<Workflow.Management.WorkflowStartProperty>
            {
                 new Workflow.Management.WorkflowStartProperty{ Name = "SuperValue", Type = WorkflowDataType.Text, Value = "SuperValueAsStartParameter"}
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
            rp =
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
                .WithUserId(EditorUserId)
                .WithUserGroups((await groupMembershipResolver.GetAllGroupsAsync(EditorUserId)).ToList())
                .WithWorkflowInfo(wf);

            var taskTransitions = await wfService.GetAllPermittedTransitionsAsync(rpTask.Build());

            var taskTransition = taskTransitions.FirstOrDefault().TransitionInfos[0];
            var taskTrigger = taskTransition.Trigger;
            rpTask.WithUserData(StateManagementCommonTriggerProperties.DataWithUserResponded(EditorUserId,
                new List<WorkflowTriggerParameter>()
                {
                    new WorkflowTriggerParameter{ Name = "Approver", Value=ApproverUserId }
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

            RegisterMock(typeof(ISafeLock), p => new TestSafeLock(p.GetRequiredService<ILogger<TestSafeLock>>()));

            var host = Host<WorkflowDatabaseTestContext>(
            new[]
            {
                RhDev.Common.Web.Core.Composition.CompositionDefinition.GetDefinition(),
                Composition.CompositionDefinition.GetDefinition()
            }, 
            useDbContextFactory: false,
            serviceConfiguration: (ctx, services) =>
            {
                services.AddScoped<IWorkflowGroupMembershipResolver, TestWorkflowGroupMembershipResolver>();
                services.AddScoped<ITestWorkflowMetadataRepository, TestWorkflowMetadataRepository>();

                services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<WorkflowDatabaseTestContext>()
                .AddDefaultTokenProviders();

            });

            return host.Services;
        }

        void RegisterEfInMemoryMock(bool seed, string? databaseName = default)
        {
            RegisterEfInMemoryMock<WorkflowDatabaseTestContext, DbContext>(ctx =>
            {
                if (seed)
                {
                    SeedDatabase(ctx);
                    ctx.SaveChanges();
                }

            }, databaseName);
        }

        void SeedDatabase(WorkflowDatabaseTestContext ctx)
        {
            ctx.AddRange(new List<IdentityRole>
                    {
                        new IdentityRole{  Id = EditorRoleId, Name = "Editor", NormalizedName="Editor" },
                        new IdentityRole{  Id = ApproverRoleId, Name = "Approver", NormalizedName="Approver" },
                        new IdentityRole{  Id = AccountantRoleId, Name = "Accountant", NormalizedName="Accountant" }
                    });


            ctx.AddRange(new List<IdentityUser>
            {
                new IdentityUser
                {
                     Id = SystemUserNames.System.ToString(),
                      UserName = "System",
                      Email = "system@system.com"
                },
                new IdentityUser
                {
                     Id = EditorUserId,
                      UserName = "EditorUser",
                      Email = "editor@contoso.com"
                },
                new IdentityUser
                {
                     Id = ApproverUserId,
                      UserName = "ApproverUser",
                      Email = "approver@contoso.com"
                },
                new IdentityUser
                {
                     Id = AccountantUserId,
                      UserName = "AccountantUser",
                      Email = "accountant@contoso.com"
                }
            });

            ctx.AddRange(new List<IdentityUserRole<string>>
            {
                new IdentityUserRole<string>
                {
                     RoleId = EditorRoleId,
                     UserId = EditorUserId
                },
                new IdentityUserRole<string>
                {
                     RoleId = ApproverRoleId,
                     UserId = ApproverUserId
                },
                new IdentityUserRole<string>
                {
                     RoleId = AccountantRoleId,
                     UserId = AccountantUserId
                }
            });

            ctx.AddRange(new List<TestWorkflowMetadata>
                    {
                        new TestWorkflowMetadata
                        {
                            CreatedById = SystemUserNames.System.ToString(),
                            InvoiceNumber = "123564987",
                            Id = 1,
                            DocumentNumber = "2024-001",
                            State = "Initial",
                            MinedSuccessfully = false
                        }
                    });
        }
    }
}