namespace RhDev.Common.Web.Core.Workflow
{
    public class WorkflowTransition
    {
        /// <summary>
        /// Id of workflow instance the transition is returned for
        /// </summary>
        public WorkflowInfo WorkflowInfo { get; set; }
        /// <summary>
        /// Display name of the Transition
        /// </summary>
        public string DisplayName { get; set; }
        /// <summary>
        /// Trigger code to call for execute workflow transition
        /// </summary>
        public string Code { get; set; }

        public IList<WorkflowTransitionParameter> TransitionParameters { get; set; }
        /// <summary>
        /// All avaliable variants for this transitions
        /// </summary>
        public IList<WorkflowTriggerVariant> Variants { get; set; }

        /// <summary>
        /// All available custom properties for workflow transition
        /// </summary>
        public List<WorkflowTransitionCustomProperty> CustomProperties { get; set; } = new();

        public static WorkflowTransition Create(string displayName, string code, List<WorkflowTransitionCustomProperty> customProperties, IList<WorkflowTransitionParameter> transitionParameters, IList<WorkflowTriggerVariant> triggerVariants, WorkflowInfo workflowInfo)
            => new WorkflowTransition
            {
                Code = code,
                DisplayName = displayName,
                CustomProperties = customProperties,
                TransitionParameters = transitionParameters,
                Variants = triggerVariants,
                WorkflowInfo = workflowInfo
            };

    }
}
