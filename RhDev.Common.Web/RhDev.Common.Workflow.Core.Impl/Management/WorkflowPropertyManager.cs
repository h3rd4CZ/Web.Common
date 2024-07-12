using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow;
using RhDev.Common.Web.Core.Utils;
using RhDev.Common.Workflow.Core.DataAccess.Sql.Repository;
using RhDev.Common.Workflow.Management;
using RhDev.Common.Workflow.PropertyModel.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RhDev.Common.Workflow.Impl.Management
{
    public class WorkflowPropertyManager : IWorkflowPropertyManager
    {
        private readonly IWorkflowInstanceRepository workflowInstanceRepository;
        private readonly IWorkflowPropertiesRepository workflowPropertiesRepository;

        public WorkflowPropertyManager(IWorkflowInstanceRepository workflowInstanceRepository,
            IWorkflowPropertiesRepository workflowPropertiesRepository)
        {
            this.workflowInstanceRepository = workflowInstanceRepository;
            this.workflowPropertiesRepository = workflowPropertiesRepository;
        }
        public async Task<StateManagementValue> LoadPropertyAsync(string name, int workflowId)
        {
            Guard.StringNotNullOrWhiteSpace(name, nameof(name));
            Guard.NotDefault(workflowId, nameof(workflowId));
                        
            var workflow = await workflowInstanceRepository.ReadByIdAsync(workflowId, new List<Expression<Func<WorkflowInstance, object>>> { l => l.WorkflowProperties });

            var property = workflow.WorkflowProperties.FirstOrDefault(p => p.Name == name);

            return Equals(null, property) ? new StateManagementNullValue() : StateManagementValue.Deserialize(property.Value, property.Type);
        }

        public async Task SavePropertyAsync(string name, StateManagementValue value, int workflowId)
        {
            Guard.StringNotNullOrWhiteSpace(name, nameof(name));
            Guard.NotNull(value, nameof(value));
            Guard.NotDefault(workflowId, nameof(workflowId));

            var valueSerialized = value.Serialize();
            var type = value.GetType().FullName;
                        
            var existProperty = (await workflowPropertiesRepository.ReadAsync(p => p.WorkflowInstanceId == workflowId && p.Name == name))?.FirstOrDefault();

            var propertyToUpdate = existProperty ?? new WorkflowProperty { };

            propertyToUpdate.WorkflowInstanceId = workflowId;
            propertyToUpdate.Name = name;
            propertyToUpdate.Type = type;
            propertyToUpdate.Value = valueSerialized;

            if (Equals(null, existProperty))
                await workflowPropertiesRepository.CreateAsync(propertyToUpdate);
            else
                await workflowPropertiesRepository.UpdateAsync(propertyToUpdate);

        }
    }
}
