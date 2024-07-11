using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhDev.Common.Web.Core.Workflow
{
    public class WorkflowExecutionTriggerData
    {
        /// <summary>
        /// Trigger parameters for transition variant
        /// </summary>
        public IList<WorkflowTriggerParameter> TriggerParameters { get; set; }
        /// <summary>
        /// Trigger code received using method for all permitted transitions
        /// </summary>        
        [Required]
        public string Trigger { get; set; }

        /// <summary>
        /// Trigger variant code received using method for all permitted transitions.
        /// If the transition has been invoking using transition variant this property must be set with variant code.
        /// If the property is not set workflow engine evaluate transition without variant
        /// </summary>
        public string WorkflowTriggerVariantCode { get; set; }


        public override string ToString()
            => $"{nameof(Trigger)}={Trigger}, {nameof(WorkflowTriggerVariantCode)}={WorkflowTriggerVariantCode}, {nameof(TriggerParameters)}=[{string.Join(",", TriggerParameters ?? Enumerable.Empty<WorkflowTriggerParameter>())}]";

    }
}
