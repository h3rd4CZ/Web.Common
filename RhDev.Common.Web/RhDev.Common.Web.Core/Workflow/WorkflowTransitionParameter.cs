using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhDev.Common.Web.Core.Workflow
{
    public class WorkflowTransitionParameter
    {
        /// <summary>
        /// Name of the property
        /// </summary>
        public string PropertyName { get; set; }
        /// <summary>
        /// Display name of the property
        /// </summary>
        public string DisplayName { get; set; }
        /// <summary>
        /// Type of the property
        /// </summary>
        public WorkflowDataType Type { get; set; }
        /// <summary>
        /// Whether the property is mandatory on transition or not, statemanagement engine checks for that
        /// </summary>
        public bool Required { get; set; }
        /// <summary>
        /// Value of the property
        /// </summary>
        public object Value { get; set; }

        public static WorkflowTransitionParameter Create(string propertyName, string displayName, WorkflowDataType type, bool required)
            => new WorkflowTransitionParameter { DisplayName = displayName, PropertyName = propertyName, Required = required, Type = type };
    }
}
