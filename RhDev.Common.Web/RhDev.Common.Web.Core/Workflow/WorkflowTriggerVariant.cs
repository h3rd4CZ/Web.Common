using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhDev.Common.Web.Core.Workflow
{
    public class WorkflowTriggerVariant
    {
        /// <summary>
        /// Display name of the transition variant
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Variant code identifies transition variant and must be set when executing transition with variant
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// All mandatory parameters to call execute workflow transition with this transition variant
        /// </summary>
        public IList<WorkflowTransitionParameter> Parameters { get; set; }
        public static WorkflowTriggerVariant Create(string displayName, string code, IList<WorkflowTransitionParameter> parameters)
            => new WorkflowTriggerVariant { DisplayName = displayName, Code = code, Parameters = parameters };
    }
}
