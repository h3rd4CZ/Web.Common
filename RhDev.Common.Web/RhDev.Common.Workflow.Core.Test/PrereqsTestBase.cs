using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RhDev.Common.Web.Core;
using RhDev.Common.Web.Core.DataAccess.Workflow;
using RhDev.Common.Web.Core.Test;
using RhDev.Common.Web.Core.Workflow;
using RhDev.Common.Workflow.Core.Test.Data;
using RhDev.Common.Workflow.DataAccess.SharePoint.Online;
using RhDev.Common.Workflow.PropertyModel.Properties;
using System.Reflection;
using Xunit.Abstractions;

namespace RhDev.Common.Workflow.Core.Test
{
    public class PrereqsTestBase : IntegrationTestBase
    {
        Guid BodUserId = Guid.NewGuid();

        public PrereqsTestBase(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

        [Fact]
        public void Composition_Base()
        {
            var services = TestProlog(true, nameof(Composition_Base));

            var dataAccessor = services.GetRequiredService<RawEntityDataAccessor>();
            var wfService = services.GetRequiredService<IWorkflowService>();
        }

        [Fact]
        public async Task RawEntityDataAccessor_Write_Base()
        {
            var services = TestProlog(true, nameof(RawEntityDataAccessor_Read_Base));

            var dataAccessor = services.GetRequiredService<IRawEntityDataAccessor>();

            var identifier = new WorkflowDocumentIdentifier(
                SectionDesignation.Empty, new WorkflowDocumentIdentificator(2, typeof(TestWorkflowMetadata).AssemblyQualifiedName!), "2024-777888", 1);

            var created = DateTime.Now;

            var sd = SectionDesignation.Empty;

            await dataAccessor.SetEntityFieldValuesAndUpdateAsync(identifier, new Dictionary<string, StateManagementValue>
            {
                { "State", new StateManagementTextValue("MinorState") },
                { "Created", new StateManagementDateTimeValue(created) },
                { "DocumentNumber", new StateManagementTextValue("2024-258") },
                { "MinedSuccessfully", new StateManagementBooleanValue(true) },
                { "WorkflowAssignedTo", new StateManagementUserValue(new UserInfo(sd, SystemUserNames.System.ToString(), "system", "", ""), sd) },
                { "CreatedById", new StateManagementUserValue(new UserInfo(sd, SystemUserNames.System.ToString(), "System", "", ""), sd) },
            });

            var repo = services.GetRequiredService<ITestWorkflowMetadataRepository>();

            var document = await repo.ReadByIdAsync(2);

            document.State.Should().Be("MinorState");
            document.Created.Should().Be(created);
            document.DocumentNumber.Should().Be("2024-258");
            document.MinedSuccessfully.Should().Be(true);
            document.WorkflowAssignedTo.Should().Be(SystemUserNames.System.ToString());
            document.CreatedById.Should().Be(SystemUserNames.System.ToString());
            document.CreatedBy.UserName.Should().Be("System");
        }

        [Fact]
        public async Task RawEntityDataAccessor_Read_Base()
        {
            var services = TestProlog(true, nameof(RawEntityDataAccessor_Read_Base));

            var dataAccessor = services.GetRequiredService<IRawEntityDataAccessor>();

            var identifier = new WorkflowDocumentIdentifier(
                SectionDesignation.Empty,
                new WorkflowDocumentIdentificator(1, typeof(TestWorkflowMetadata).AssemblyQualifiedName!), "2024-777888", 1);

            var smIdValue = await dataAccessor.GetEntityFieldValueAsync(identifier, "Id");
                        
            var smCreatedValue = await dataAccessor.GetEntityFieldValueAsync(identifier, "Created");
            var smDocNumberValue = await dataAccessor.GetEntityFieldValueAsync(identifier, "DocumentNumber");
            var smInvoiceNumber = await dataAccessor.GetEntityFieldValueAsync(identifier, "InvoiceNumber");
            var smCreatedByIdValue = await dataAccessor.GetEntityFieldValueAsync(identifier, "CreatedById");
            var smAssignedToValue = await dataAccessor.GetEntityFieldValueAsync(identifier, "WorkflowAssignedTo");

            smAssignedToValue.Should().BeOfType<StateManagementUserValue>();
            smCreatedByIdValue.Should().BeOfType<StateManagementUserValue>();
            smIdValue.Should().NotBeNull().And.BeOfType<StateManagementNumberValue>();
            smCreatedValue.Should().NotBeNull().And.BeOfType<StateManagementNullValue>();
            smDocNumberValue.Should().NotBeNull().And.BeOfType<StateManagementTextValue>();
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
                services.AddScoped<ITestWorkflowMetadataRepository, TestWorkflowMetadataRepository>();
            }); ;

            var services = host.Services;

            return services;
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

        void SeedDatabase(DbContext ctx)
        {
           
            ctx.AddRange(new List<IdentityUser>
            {
                new IdentityUser
                {
                     Id = SystemUserNames.System.ToString(),
                      UserName = "System",
                      Email = "system@workflow.com"
                },
                new IdentityUser
                {
                     Id = BodUserId.ToString(),
                      UserName = "Bob",
                      Email = "bob@workflow.com"
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